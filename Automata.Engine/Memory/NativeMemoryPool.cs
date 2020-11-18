using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Memory
{
    public unsafe class NativeMemoryPool
    {
        private sealed record MemoryBlock(nuint Index, nuint Size, bool Owned);

        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly object _AccessLock;
        private readonly byte* _Pointer;

        public nuint Size { get; }

        private int _RentedBlocks;
        public int RentedBlocks => _RentedBlocks;

        public NativeMemoryPool(byte* pointer, nuint size)
        {
            Size = size;

            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, size, false));
            _AccessLock = new object();
            _Pointer = pointer;
        }

        /// <summary>
        ///     Attempts to rent a sector of memory from the pool.
        /// </summary>
        /// <param name="size">Size of the rental in units of <see cref="T" />.</param>
        /// <param name="alignment">The alignment of the rental index in bytes.</param>
        /// <param name="index">The resulting index of the rental, from the start of the pool's pointer.</param>
        /// <param name="clear">Whether to clear the resulting rental before returning the <see cref="IMemoryOwner{T}" />.</param>
        /// <typeparam name="T">The unmanaged type to return <see cref="IMemoryOwner{T}" /> as.</typeparam>
        /// <returns></returns>
        public IMemoryOwner<T> Rent<T>(int size, nuint alignment, [MaybeNullWhen(false)] out nuint index, bool clear = false) where T : unmanaged
        {
            if (size < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), "Size must be non-negative.");
            }

            // `byteSize` will be used to define the MemoryBlock sizes
            // it's assumed that `size` will be in units of `T`, so to properly
            // align memory block sizes, we need a byte-length representation
            // of the provided `size`.
            nuint sizeInBytes = (nuint)size * (nuint)sizeof(T);

            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;

                if (current is null)
                {
                    ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");
                }

                do
                {
                    if (current!.Value.Owned)
                    {
                        continue;
                    }

                    if (alignment is 0u)
                    {
                        // just convert entire block to owned
                        if (current.Value.Size == sizeInBytes)
                        {
                            current.Value = current.Value with { Owned = true };
                        }
                        else if (current.Value.Size > sizeInBytes)
                        {
                            nuint afterBlockIndex = current.Value.Index + sizeInBytes;
                            nuint afterBlockLength = current.Value.Size - sizeInBytes;

                            // collapse current block to correct length
                            current.Value = current.Value with { Size = sizeInBytes, Owned = true };

                            // allocate new block after current with remaining length
                            _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        nuint alignmentPadding = (alignment - (current.Value.Index % alignment)) % alignment;
                        nuint alignedIndex = current.Value.Index + alignmentPadding;
                        nuint alignedSize = current.Value.Size - alignmentPadding;

                        // check for an overflow, in which case size is too small.
                        if (alignedSize > current.Value.Size)
                        {
                            continue;
                        }
                        else if ((alignedIndex == current.Value.Index) && (alignedSize == sizeInBytes))
                        {
                            current.Value = current.Value with { Owned = true };
                        }
                        else if (alignedSize >= sizeInBytes)
                        {
                            // if our alignment forces us out-of-alignment with
                            // this block's index, then allocate a block before to
                            // facilitate the unaligned size
                            if (alignedIndex > current.Value.Index)
                            {
                                nuint beforeBlockIndex = current.Value.Index;
                                nuint beforeBlockSize = alignmentPadding;

                                _MemoryMap.AddBefore(current, new MemoryBlock(beforeBlockIndex, beforeBlockSize, false));
                                current.Value = current.Value with { Index = alignedIndex, Size = sizeInBytes, Owned = true };
                            }

                            nuint afterBlockIndex = current.Value.Index + sizeInBytes;
                            nuint afterBlockSize = current.Value.Size - sizeInBytes;

                            current.Value = current.Value with { Size = sizeInBytes, Owned = true };
                            _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockSize, false));
                        }
                        else
                        {
                            continue;
                        }
                    }

                    index = current.Value.Index;
                    IMemoryOwner<T> memoryOwner = CreateMemoryOwner<T>(index, size);

                    if (clear)
                    {
                        memoryOwner.Memory.Span.Clear();
                    }

                    return memoryOwner;
                } while ((current = current!.Next) is not null);
            }

            index = default;
            ThrowHelper.ThrowInsufficientMemoryException("Not enough memory to accomodate allocation.");
            return null!;
        }

        /// <summary>
        ///     Creates an <see cref="IMemoryOwner{T}" /> from the specified index and size.
        /// </summary>
        /// <param name="index">Index of which <see cref="IMemoryOwner{T}" /> starts (relative to start of buffer).</param>
        /// <param name="size">Size of <see cref="IMemoryOwner{T}" /> in units of <see cref="T" />.</param>
        /// <typeparam name="T">Unmanaged type to instance <see cref="IMemoryOwner{T}" /> with.</typeparam>
        /// <returns>
        ///     <see cref="IMemoryOwner{T}" /> wrapping a specified region of memory, starting at <see cref="index" /> and ending
        ///     at <see cref="index" /> + <see cref="Size" />.
        /// </returns>
        private IMemoryOwner<T> CreateMemoryOwner<T>(nuint index, int size) where T : unmanaged
        {
            if (size > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), $"Length cannot be greater than {int.MaxValue}.");
            }

            // remark: it's POSSIBLE for alignment to get screwed up in this operation.
            // T will not often be the same size as _Pointer (i.e. a byte), so it's important to take care in calling this
            // method with a valid index and size that won't misalign

            NativeMemoryManager<T> memoryManager = new NativeMemoryManager<T>((T*)(_Pointer + index), size);
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, index, memoryManager.Memory);
            Interlocked.Increment(ref _RentedBlocks);

            return memoryOwner;
        }

        internal void Return<T>(NativeMemoryOwner<T> memoryOwner) where T : unmanaged
        {
            LinkedListNode<MemoryBlock> GetMemoryBlockAtIndex(nuint index)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;

                if (current is null)
                {
                    ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");
                }

                do
                {
                    if (current!.Value.Index == index)
                    {
                        return current;
                    }
                } while ((current = current!.Next) is not null);

                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index), "No memory block starts at index.");
                return null!; // this return should never be hit
            }

            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock> current = GetMemoryBlockAtIndex(memoryOwner.Index);
                LinkedListNode<MemoryBlock>? before = current.Previous;
                LinkedListNode<MemoryBlock>? after = current.Next;
                current.Value = current.Value with { Owned = false };

                if (before?.Value.Owned is false)
                {
                    nuint newIndex = before.Value.Index;
                    nuint newLength = before.Value.Size + current.Value.Size;
                    current.Value = current.Value with { Index = newIndex, Size = newLength };
                }

                if (after?.Value.Owned is false)
                {
                    nuint newLength = current.Value.Size + after.Value.Size;
                    current.Value = current.Value with { Size = newLength };
                }
            }

            Interlocked.Decrement(ref _RentedBlocks);
        }
    }
}
