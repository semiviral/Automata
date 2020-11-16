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

        public IMemoryOwner<T> Rent<T>(int size, [MaybeNullWhen(false)] out nuint index, bool clear = false) where T : unmanaged
        {
            if (size < 0) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), "Size must be non-negative.");

            // `byteSize` will be used to define the MemoryBlock sizes
            // it's assumed that `size` will be in units of `T`, so to properly
            // align memory block sizes, we need a byte-length representation
            // of the provided `size`.
            nuint sizeInBytes = (nuint)size * (nuint)sizeof(T);

            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
                if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

                do
                {
                    if (current!.Value.Owned) continue;

                    // just convert entire block to owned
                    if (current.Value.Size == sizeInBytes) current.Value = current.Value with { Owned = true };
                    else if (current.Value.Size > sizeInBytes)
                    {
                        nuint afterBlockIndex = current.Value.Index + sizeInBytes;
                        nuint afterBlockLength = current.Value.Size - sizeInBytes;

                        // collapse current block to correct length
                        current.Value = current.Value with { Size = sizeInBytes, Owned = true };

                        // allocate new block after current with remaining length
                        _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));
                    }
                    else continue;

                    IMemoryOwner<T> memoryOwner = CreateMemoryOwner<T>(current.Value.Index, size);
                    if (clear) memoryOwner.Memory.Span.Clear();
                    index = current.Value.Index;
                    return memoryOwner;
                } while ((current = current!.Next) is not null);
            }

            index = default;
            ThrowHelper.ThrowInsufficientMemoryException("Not enough memory to accomodate allocation.");
            return null!;
        }

        private IMemoryOwner<T> CreateMemoryOwner<T>(nuint index, int size) where T : unmanaged
        {
            if (size > int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), $"Length cannot be greater than {int.MaxValue}.");

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
                if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

                do
                {
                    if (current!.Value.Index == index) return current;
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
