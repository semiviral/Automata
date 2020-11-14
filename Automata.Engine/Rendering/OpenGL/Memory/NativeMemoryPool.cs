using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Extensions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    public unsafe class NativeMemoryPool
    {
        private sealed record MemoryBlock
        {
            public nuint Index { get; init; }
            public nuint Length { get; init; }
            public bool Owned { get; init; }

            public MemoryBlock(nuint index, nuint length, bool owned) => (Index, Length, Owned) = (index, length, owned);
        }

        private readonly NativeMemoryManager<byte>? _MemoryManager;
        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly object _AccessLock;
        private readonly byte* _Pointer;

        private int _RentedBlocks;
        public int RentedBlocks => _RentedBlocks;

        public NativeMemoryPool(byte* pointer, nuint length)
        {
            // one memory instance can't be >int.MaxValue, so ensure length is less or equal
            // if it isn't, the default memory manager is uninitialized, and we just use a new memory manager
            // for each rent
            if (length <= int.MaxValue) _MemoryManager = new NativeMemoryManager<byte>(pointer, (int)length);

            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, length, false));
            _AccessLock = new object();
            _Pointer = pointer;
        }

        public IMemoryOwner<T> Rent<T>(nuint length) where T : unmanaged
        {
            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
                if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

                do
                {
                    if (current!.Value.Owned) continue;

                    // just convert entire block to owned
                    if (current.Value.Length == length) current.Value = current.Value with { Owned = true };
                    else if (current.Value.Length > length)
                    {
                        nuint afterBlockIndex = current.Value.Index + length;
                        nuint afterBlockLength = current.Value.Length - length;

                        // collapse current block to correct length
                        current.Value = current.Value with { Length = length, Owned = true };

                        // allocate new block with rest of length
                        _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));
                    }
                    else continue;

                    return _MemoryManager is not null
                        ? CreateMemoryOwnerFromBlockWithSlice<T>(current.Value)
                        : CreateMemoryOwnerFromBlockWithNewManager<T>(current.Value);
                } while ((current = current.Next) is not null);
            }

            throw new InsufficientMemoryException("Not enough memory to accomodate allocation.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMemoryOwner<T> CreateMemoryOwnerFromBlockWithSlice<T>(MemoryBlock memoryBlock) where T : unmanaged
        {
            if (_MemoryManager is null) ThrowHelper.ThrowInvalidOperationException("No memory manager to slice memory from.");
            else if (memoryBlock.Index >= int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Index));
            else if (memoryBlock.Length > int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Length));

            Memory<T> memory = _MemoryManager!.Slice((int)memoryBlock.Index, (int)memoryBlock.Length).Cast<byte, T>();
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, memoryBlock.Index, memory);
            Interlocked.Increment(ref _RentedBlocks);

            return memoryOwner;
        }

        private IMemoryOwner<T> CreateMemoryOwnerFromBlockWithNewManager<T>(MemoryBlock memoryBlock) where T : unmanaged
        {
            if (memoryBlock.Length > int.MaxValue)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(memoryBlock.Length), $"Length cannot be greater than {int.MaxValue}.");

            T* offsetPointer = (T*)_Pointer + memoryBlock.Index;
            NativeMemoryManager<T> memoryManager = new NativeMemoryManager<T>(offsetPointer, (int)memoryBlock.Length);
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
                current.Value = current.Value with { Owned = false};

                if (before?.Value.Owned is false)
                {
                    nuint newIndex = before.Value.Index;
                    nuint newLength = before.Value.Length + current.Value.Length;
                    current.Value = current.Value with { Index = newIndex, Length = newLength };
                    _MemoryMap.Remove(before);
                }

                if (after?.Value.Owned is false)
                {
                    nuint newLength = current.Value.Length + after.Value.Length;
                    current.Value = current.Value with { Length = newLength };
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
            } while ((current = current.Next) is not null);

            throw new InsufficientMemoryException("No memory block starts at index.");
        }
    }
}
