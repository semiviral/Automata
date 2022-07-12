using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Memory
{
    public unsafe class NativeMemoryPool
    {
        /// <summary>
        ///     Record used to track memory allocations within the pool.
        /// </summary>
        private sealed record MemoryBlock(nuint Index, nuint Size, bool Owned);

        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly object _AccessLock;
        private readonly byte* _Pointer;

        /// <summary>
        ///     Size (in bytes) of the memory pool.
        /// </summary>
        /// <remarks>
        ///     This value is immutable, externally and internally.
        /// </remarks>
        public nuint Size { get; }

        /// <summary>
        ///     Rented size (in bytes) from the memory pool.
        /// </summary>
        public nuint RentedSize { get; private set; }

        /// <summary>
        ///     Remaining size (in bytes) in the memory pool.
        /// </summary>
        public nuint RemainingSize => Size - RentedSize;

        /// <summary>
        ///     Count of the current owned blocks in the pool.
        /// </summary>
        public int RentedBlocks { get; private set; }

        public NativeMemoryPool(byte* pointer, nuint size)
        {
            _Pointer = pointer;
            _AccessLock = new object();
            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, size, false));

            Size = size;
            RentedSize = (nuint)0u;
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

                foreach (MemoryBlock memory_block in _MemoryMap)
                {
                    if (memory_block.Index != index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(memory_block.Index), $"{nameof(MemoryBlock)} index does not follow previous block.");
                    }
                    else
                    {
                        index += memory_block.Size;
                    }
                }
            }
        }

        public override string ToString() => $"{nameof(NativeMemoryPool)}(MemoryBlocks {RentedBlocks}, {RentedSize}/{Size})";


        #region Rent

        /// <summary>
        ///     Attempts to rent a sector of memory from the pool.
        /// </summary>
        /// <param name="size">Size of the rental in units of <see cref="T" />.</param>
        /// <param name="alignment">The alignment of the rental index in bytes. This can be 0u for no alignment.</param>
        /// <param name="index">The resulting index of the rental, from the start of the pool's pointer.</param>
        /// <param name="clear">Whether to clear the resulting rental before returning the <see cref="IMemoryOwner{T}" />.</param>
        /// <typeparam name="T">The unmanaged type to return <see cref="IMemoryOwner{T}" /> as.</typeparam>
        /// <returns><see cref="IMemoryOwner{T}" /> wrapping an arbitrary region of pool memory.</returns>
        public IMemoryOwner<T> Rent<T>(int size, nuint alignment, [MaybeNullWhen(false)] out nuint index, bool clear = false) where T : unmanaged
        {
            index = 0u;

            switch (size)
            {
                case < 0: throw new ArgumentOutOfRangeException(nameof(size), "Size must be non-negative and less than the size of the pool.");
                case 0: return new NativeMemoryPoolOwner<T>(this, 0, Memory<T>.Empty);
            }

            lock (_AccessLock)
            {
                nuint size_in_bytes = (nuint)size * (nuint)sizeof(T);

                if (size_in_bytes <= RemainingSize)
                {
                    for (LinkedListNode<MemoryBlock>? current = SafeGetFirstNode(); current is not null; current = current.Next)
                    {
                        // The third argument will be used to define the MemoryBlock sizes. It's assumed that `size` will
                        // be in units of `T`, so to properly align memory blocks, we need a byte-length representation of
                        // the provided `size` parameter's value.

                        if (!TryAllocateMemoryBlock(current, alignment, size_in_bytes))
                        {
                            continue;
                        }

                        index = current.Value.Index;
                        IMemoryOwner<T> memory_owner = CreateMemoryOwner<T>(index, size);
                        RentedSize += size_in_bytes;
                        RentedBlocks += 1;

                        if (clear)
                        {
                            // this can be pretty expensive, so make sure we only clear when the user wants to
                            memory_owner.Memory.Span.Clear();
                        }

                        return memory_owner;
                    }
                }
            }

            // at this point, we've rented zero blocks and iterated them all, so we know it's not possible to accomodate the allocation.
            throw new InsufficientMemoryException("Not enough memory to accomodate allocation.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAllocateMemoryBlock(LinkedListNode<MemoryBlock> current, nuint alignment, nuint sizeInBytes) =>
            !current.Value.Owned
            && (alignment is not 0u || TryAllocateMemoryBlockWithoutAlignment(current, sizeInBytes))
            && (alignment is 0u || TryAllocateMemoryBlockWithAlignment(current, alignment, sizeInBytes));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAllocateMemoryBlockWithoutAlignment(LinkedListNode<MemoryBlock> current, nuint sizeInBytes)
        {
            Debug.Assert(!current.Value.Owned);

            // just convert entire block to owned
            if (current.Value.Size == sizeInBytes)
            {
                current.Value = current.Value with { Owned = true };
            }
            else if (current.Value.Size > sizeInBytes)
            {
                // allocate new block after current with remaining length
                nuint after_block_index = current.Value.Index + sizeInBytes;
                nuint after_block_length = current.Value.Size - sizeInBytes;
                _MemoryMap.AddAfter(current, new MemoryBlock(after_block_index, after_block_length, false));

                // modify current block to reflect proper size
                current.Value = current.Value with { Size = sizeInBytes, Owned = true };
            }
            else
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAllocateMemoryBlockWithAlignment(LinkedListNode<MemoryBlock> current, nuint alignment, nuint sizeInBytes)
        {
            Debug.Assert(!current.Value.Owned);

            nuint alignment_padding = (alignment - (current.Value.Index % alignment)) % alignment;
            nuint aligned_index = current.Value.Index + alignment_padding;
            nuint aligned_size = current.Value.Size - alignment_padding;

            // check for an overflow, in which case size is too small.
            if (aligned_size > current.Value.Size)
            {
                return false;
            }
            else if (alignment_padding is 0u && (current.Value.Size == sizeInBytes))
            {
                current.Value = current.Value with { Owned = true };
            }
            else if (aligned_size >= sizeInBytes)
            {
                // if our alignment forces us out-of-alignment with
                // this block's index, then allocate a block before to
                // facilitate the unaligned size
                if (aligned_index > current.Value.Index)
                {
                    nuint before_block_index = current.Value.Index;
                    nuint before_block_size = alignment_padding;
                    MemoryBlock before_block = new MemoryBlock(before_block_index, before_block_size, false);
                    _MemoryMap.AddBefore(current, before_block);

                    nuint new_current_size = current.Value.Size - alignment_padding;
                    current.Value = current.Value with { Index = aligned_index, Size = new_current_size, Owned = true };
                }

                // allocate block after current to hold remaining length
                nuint after_block_index = current.Value.Index + sizeInBytes;
                nuint after_block_size = current.Value.Size - sizeInBytes;
                MemoryBlock after_block = new MemoryBlock(after_block_index, after_block_size, false);
                _MemoryMap.AddAfter(current, after_block);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMemoryOwner<T> CreateMemoryOwner<T>(nuint index, int size) where T : unmanaged
        {
            // remark: it's POSSIBLE for alignment to get screwed up in this operation.
            // T will not often be the same size as _Pointer (i.e. a byte), so it's important to take care in calling this
            // method with a valid index and size that won't misalign. With this in mind, ensure that instantiating the
            // NativeMemoryManager ALWAYS uses the units-of-T size.
            NativeMemoryManager<T> memory_manager = new NativeMemoryManager<T>((T*)(_Pointer + index), size);
            IMemoryOwner<T> memory_owner = new NativeMemoryPoolOwner<T>(this, index, memory_manager.Memory);

            return memory_owner;
        }

        #endregion


        #region Return

        internal void Return<T>(NativeMemoryPoolOwner<T> memoryPoolOwner) where T : unmanaged
        {
            Debug.Assert(!memoryPoolOwner.Memory.IsEmpty,
                $"{nameof(IMemoryOwner<T>)} has already been disposed, does not belong to this pool, or does not have a real block allocated.");

            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock> current = GetMemoryBlockAtIndex(memoryPoolOwner.Index);
                LinkedListNode<MemoryBlock>? before = current.Previous;
                LinkedListNode<MemoryBlock>? after = current.Next;
                current.Value = current.Value with { Owned = false };
                RentedSize -= current.Value.Size;
                RentedBlocks -= 1;

                // attempt to merge current block & its precedent node
                if (before?.Value.Owned is false)
                {
                    nuint new_index = before.Value.Index;
                    nuint new_size = before.Value.Size + current.Value.Size;
                    current.Value = current.Value with { Index = new_index, Size = new_size };
                    _MemoryMap.Remove(before);
                }

                // attempt to merge current block & its antecedent node
                if (after?.Value.Owned is false)
                {
                    nuint new_length = current.Value.Size + after.Value.Size;
                    current.Value = current.Value with { Size = new_length };
                    _MemoryMap.Remove(after);
                }

                Debug.Assert(RemainingSize <= Size, "Remaining size is should never be greater than the pool size.");
            }
        }

        private LinkedListNode<MemoryBlock> GetMemoryBlockAtIndex(nuint index)
        {
            for (LinkedListNode<MemoryBlock>? current = SafeGetFirstNode(); current is not null; current = current.Next)
            {
                if (current.Value.Index == index)
                {
                    return current;
                }
            }

            throw new InvalidOperationException($"No block exists for memory owner: {index}.");
        }

        #endregion
    }
}
