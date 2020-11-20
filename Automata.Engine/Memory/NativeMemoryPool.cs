using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Memory
{
    public unsafe class NativeMemoryPool
    {
        /// <summary>
        ///     Record used to track memory allocations within the pool.
        /// </summary>
        private sealed record MemoryBlock(nuint Index, nuint Size, bool Owned);

        private readonly object _AccessLock;

        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly byte* _Pointer;

        private int _RentedBlocks;

        /// <summary>
        ///     Size (in bytes) of the memory pool.
        /// </summary>
        /// <remarks>
        ///     This value is immutable, externally and internally.
        /// </remarks>
        public nuint Size { get; }

        /// <summary>
        ///     Count of the current owned blocks in the pool.
        /// </summary>
        public int RentedBlocks => _RentedBlocks;

        public NativeMemoryPool(byte* pointer, nuint size)
        {
            Size = size;

            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, size, false));
            _AccessLock = new object();
            _Pointer = pointer;
        }

        private LinkedListNode<MemoryBlock> SafeGetFirstNode() => _MemoryMap.First ?? throw new InvalidOperationException("Memory pool is in invalid state.");

        /// <summary>
        ///     Validates all memory blocks currently in the pool. This is a diagnostic method.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a block's index doesn't match the previous block's index + size.</exception>
        public void ValidateBlocks()
        {
            lock (_AccessLock)
            {
                nuint index = 0;

                foreach (MemoryBlock memoryBlock in _MemoryMap)
                {
                    if (memoryBlock.Index != index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(memoryBlock.Index), $"{nameof(MemoryBlock)} index does not follow previous block.");
                    }
                    else
                    {
                        index += memoryBlock.Size;
                    }
                }
            }
        }


        #region Rent

        /// <summary>
        ///     Attempts to rent a sector of memory from the pool.
        /// </summary>
        /// <param name="size">Size of the rental in units of <see cref="T" />.</param>
        /// <param name="alignment">The alignment of the rental index in bytes.</param>
        /// <param name="index">The resulting index of the rental, from the start of the pool's pointer.</param>
        /// <param name="clear">Whether to clear the resulting rental before returning the <see cref="IMemoryOwner{T}" />.</param>
        /// <typeparam name="T">The unmanaged type to return <see cref="IMemoryOwner{T}" /> as.</typeparam>
        /// <returns><see cref="IMemoryOwner{T}" /> wrapping an arbitrary region of pool memory.</returns>
        public IMemoryOwner<T> Rent<T>(int size, nuint alignment, [MaybeNullWhen(false)] out nuint index, bool clear = false) where T : unmanaged
        {
            index = 0u;

            switch (size)
            {
                case < 0: throw new ArgumentOutOfRangeException(nameof(size), "Size must be non-negative.");
                case 0: return new NativeMemoryOwner<T>(this, 0, Memory<T>.Empty);
            }

            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = SafeGetFirstNode();

                do
                {
                    // The third argument will be used to define the MemoryBlock sizes. It's assumed that `size` will
                    // be in units of `T`, so to properly align memory blocks, we need a byte-length representation of
                    // the provided `size` parameter's value.
                    if (!TryAllocateMemoryBlock(current!, alignment, (nuint)size * (nuint)sizeof(T)))
                    {
                        continue;
                    }

                    index = current.Value.Index;
                    IMemoryOwner<T> memoryOwner = CreateMemoryOwner<T>(index, size);

                    if (clear)
                    {
                        // this can be pretty expensive, so make sure we only clear when the user wants to
                        memoryOwner.Memory.Span.Clear();
                    }

                    // make sure we decrement the rented blocks counter
                    Interlocked.Increment(ref _RentedBlocks);
                    return memoryOwner;
                } while ((current = current!.Next) is not null);
            }

            // at this point, we've rented zero blocks and iterated them all, so we know it's not possible to accomodate the allocation.
            throw new InsufficientMemoryException("Not enough memory to accomodate allocation.");
        }

        private bool TryAllocateMemoryBlock(LinkedListNode<MemoryBlock> current, nuint alignment, nuint sizeInBytes)
        {
            switch (alignment)
            {
                case { } when current!.Value.Owned:
                case 0u when !TryAllocateMemoryBlockWithoutAlignment(current, sizeInBytes):
                case not 0u when !TryAllocateMemoryBlockWithAlignment(current, alignment, sizeInBytes): return false;
            }

            return true;
        }

        private bool TryAllocateMemoryBlockWithoutAlignment(LinkedListNode<MemoryBlock> current, nuint sizeInBytes)
        {
            // just convert entire block to owned
            if (current.Value.Size == sizeInBytes)
            {
                current.Value = current.Value with { Owned = true };
            }
            else if (current.Value.Size > sizeInBytes)
            {
                // allocate new block after current with remaining length
                nuint afterBlockIndex = current.Value.Index + sizeInBytes;
                nuint afterBlockLength = current.Value.Size - sizeInBytes;
                _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));

                // modify current block to reflect proper size
                current.Value = current.Value with { Size = sizeInBytes, Owned = true };
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool TryAllocateMemoryBlockWithAlignment(LinkedListNode<MemoryBlock> current, nuint alignment, nuint sizeInBytes)
        {
            nuint alignmentPadding = (alignment - (current.Value.Index % alignment)) % alignment;
            nuint alignedIndex = current.Value.Index + alignmentPadding;
            nuint alignedSize = current.Value.Size - alignmentPadding;

            // check for an overflow, in which case size is too small.
            if (alignedSize > current.Value.Size)
            {
                return false;
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
                    nuint newCurrentSize = current.Value.Size - alignmentPadding;

                    _MemoryMap.AddBefore(current, new MemoryBlock(beforeBlockIndex, beforeBlockSize, false));
                    current.Value = current.Value with { Index = alignedIndex, Size = newCurrentSize, Owned = true };
                }

                // allocate block after current to hold remaining length
                nuint afterBlockIndex = current.Value.Index + sizeInBytes;
                nuint afterBlockSize = current.Value.Size - sizeInBytes;
                _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockSize, false));

                // modify current block to reflect proper size
                current.Value = current.Value with { Size = sizeInBytes, Owned = true };
            }
            else
            {
                return false;
            }

            return true;
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
            // remark: it's POSSIBLE for alignment to get screwed up in this operation.
            // T will not often be the same size as _Pointer (i.e. a byte), so it's important to take care in calling this
            // method with a valid index and size that won't misalign. With this in mind, ensure that instantiating the
            // NativeMemoryManager ALWAYS uses the units-of-T size.
            NativeMemoryManager<T> memoryManager = new((T*)(_Pointer + index), size);
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, index, memoryManager.Memory);

            return memoryOwner;
        }

        #endregion


        #region Return

        internal void Return<T>(NativeMemoryOwner<T> memoryOwner) where T : unmanaged
        {
            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock> current = GetMemoryBlockAtIndex(memoryOwner.Index);
                LinkedListNode<MemoryBlock>? before = current.Previous;
                LinkedListNode<MemoryBlock>? after = current.Next;
                current.Value = current.Value with { Owned = false };

                // attempt to merge current block & its precedent node
                if (before?.Value.Owned is false)
                {
                    nuint newIndex = before.Value.Index;
                    nuint newLength = before.Value.Size + current.Value.Size;
                    current.Value = current.Value with { Index = newIndex, Size = newLength };
                    _MemoryMap.Remove(before);
                }

                // attempt to merge current block & its antecedent node
                if (after?.Value.Owned is false)
                {
                    nuint newLength = current.Value.Size + after.Value.Size;
                    current.Value = current.Value with { Size = newLength };
                    _MemoryMap.Remove(after);
                }
            }

            // make sure we decrement the rented blocks counter
            Interlocked.Decrement(ref _RentedBlocks);
        }

        private LinkedListNode<MemoryBlock> GetMemoryBlockAtIndex(nuint index)
        {
            LinkedListNode<MemoryBlock>? current = SafeGetFirstNode();

            do
            {
                if (current!.Value.Index == index)
                {
                    return current;
                }
            } while ((current = current!.Next) is not null);

            throw new ArgumentOutOfRangeException(nameof(index), "No memory block starts at index.");
        }

        #endregion
    }
}
