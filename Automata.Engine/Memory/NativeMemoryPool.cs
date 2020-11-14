using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Extensions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Memory
{
    public unsafe class NativeMemoryPool
    {
        private sealed record MemoryBlock
        {
            public nuint Index { get; init; }
            public nuint Size { get; init; }
            public bool Owned { get; init; }

            public MemoryBlock(nuint index, nuint size, bool owned) => (Index, Size, Owned) = (index, size, owned);
        }

        private readonly NativeMemoryManager<byte>? _MemoryManager;
        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly object _AccessLock;
        private readonly byte* _Pointer;

        public nuint Size { get; }

        private int _RentedBlocks;
        public int RentedBlocks => _RentedBlocks;

        public NativeMemoryPool(byte* pointer, nuint size)
        {
            // one memory instance can't be >int.MaxValue, so ensure length is less or equal
            // if it isn't, the default memory manager is uninitialized, and we just use a new memory manager
            // for each rent
            if (size <= int.MaxValue) _MemoryManager = new NativeMemoryManager<byte>(pointer, (int)size);

            Size = size;

            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, size, false));
            _AccessLock = new object();
            _Pointer = pointer;
        }

        public IMemoryOwner<T> Rent<T>(nuint size, bool clear = false) where T : unmanaged
        {
            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
                if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

                do
                {
                    if (current!.Value.Owned) continue;

                    // just convert entire block to owned
                    if (current.Value.Size == size) current.Value = current.Value with { Owned = true };
                    else if (current.Value.Size > size)
                    {
                        nuint afterBlockIndex = current.Value.Index + size;
                        nuint afterBlockLength = current.Value.Size - size;

                        // collapse current block to correct length
                        current.Value = current.Value with { Size = size, Owned = true };

                        // allocate new block with rest of length
                        _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));
                    }
                    else continue;

                    IMemoryOwner<T> memoryOwner = _MemoryManager is not null // if memory manager isn't null then the allocation is <=int.MaxValue
                        ? CreateMemoryOwnerFromBlockWithSlice<T>(current.Value)
                        : CreateMemoryOwnerFromBlockWithNewManager<T>(current.Value);

                    if (clear) memoryOwner.Memory.Span.Clear();
                    return memoryOwner;
                } while ((current = current?.Next) is not null);
            }

            ThrowHelper.ThrowInsufficientMemoryException("Not enough memory to accomodate allocation.");
            return null!;
        }

        private IMemoryOwner<T> CreateMemoryOwnerFromBlockWithSlice<T>(MemoryBlock memoryBlock) where T : unmanaged
        {
            if (_MemoryManager is null) ThrowHelper.ThrowInvalidOperationException("No memory manager to slice memory from.");
            else if (memoryBlock.Index >= int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Index));
            else if (memoryBlock.Size > int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Size));

            Memory<T> memory = _MemoryManager!.Slice((int)memoryBlock.Index, (int)memoryBlock.Size * sizeof(T)).Cast<byte, T>();
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, memoryBlock.Index, memory);
            Interlocked.Increment(ref _RentedBlocks);

            return memoryOwner;
        }

        private IMemoryOwner<T> CreateMemoryOwnerFromBlockWithNewManager<T>(MemoryBlock memoryBlock) where T : unmanaged
        {
            if (memoryBlock.Size > int.MaxValue)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Size), $"Length cannot be greater than {int.MaxValue}.");

            // remark: it's POSSIBLE for alignment to get screwed up in this operation.
            // T will not often be the same size as _Pointer, so it's important to take care in calling this
            // method with a valid MemoryBlock that won't misalign other MemoryBlock's offsets and length.
            T* offsetPointer = (T*)(_Pointer + memoryBlock.Index);
            NativeMemoryManager<T> memoryManager = new NativeMemoryManager<T>(offsetPointer, (int)memoryBlock.Size * sizeof(T));
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, memoryBlock.Index, memoryManager.Memory);
            Interlocked.Increment(ref _RentedBlocks);

            return memoryOwner;
        }

        internal void Return<T>(NativeMemoryOwner<T> memoryOwner) where T : unmanaged
        {
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
                    current.Value = current.Value with{ Index = newIndex, Size = newLength };
                    _MemoryMap.Remove(before);
                }

                if (after?.Value.Owned is false)
                {
                    nuint newLength = current.Value.Size + after.Value.Size;
                    current.Value = current.Value with { Size = newLength };
                    _MemoryMap.Remove(after);
                }
            }

            Interlocked.Decrement(ref _RentedBlocks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LinkedListNode<MemoryBlock> GetMemoryBlockAtIndex(nuint index)
        {
            LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
            if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

            do
            {
                if (current!.Value.Index == index) return current;
            } while ((current = current?.Next) is not null);

            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index), "No memory block starts at index.");
            return null!;
        }
    }
}
