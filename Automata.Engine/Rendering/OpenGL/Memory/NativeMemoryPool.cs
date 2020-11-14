using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.HighPerformance.Extensions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    public unsafe class NativeMemoryPool
    {
        private sealed record MemoryBlock
        {
            public uint Index { get; init; }
            public uint Length { get; init; }
            public bool Owned { get; init; }

            public MemoryBlock(uint index, uint length, bool owned) => (Index, Length, Owned) = (index, length, owned);
        }

        private readonly NativeMemoryManager<byte> _MemoryManager;
        private readonly LinkedList<MemoryBlock> _MemoryMap;
        private readonly object _AccessLock;

        public NativeMemoryPool(byte* pointer, uint length)
        {
            _MemoryManager = new NativeMemoryManager<byte>(pointer, length);
            _MemoryMap = new LinkedList<MemoryBlock>();
            _MemoryMap.AddFirst(new MemoryBlock(0u, length, false));
            _AccessLock = new object();
        }

        public IMemoryOwner<T> Rent<T>(uint length) where T : unmanaged
        {
            lock (_AccessLock)
            {
                LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
                if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

                do
                {
                    if (current!.Value.Owned) continue;

                    if (current.Value.Length == length)
                    {
                        // just convert entire block to owned
                        current.Value = current.Value with { Owned = true };

                        return CreateMemoryOwnerFromBlock<T>(current.Value);
                    }
                    else if (current.Value.Length > length)
                    {
                        uint afterBlockIndex = current.Value.Index + length;
                        uint afterBlockLength = current.Value.Length - length;

                        // collapse current block to correct length
                        current.Value = current.Value with { Length = length, Owned = true };

                        // allocate new block with rest of length
                        _MemoryMap.AddAfter(current, new MemoryBlock(afterBlockIndex, afterBlockLength, false));

                        return CreateMemoryOwnerFromBlock<T>(current.Value);
                    }
                } while ((current = current?.Next) is not null);
            }

            throw new InsufficientMemoryException("Not enough memory to accomodate allocation.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IMemoryOwner<T> CreateMemoryOwnerFromBlock<T>(MemoryBlock memoryBlock) where T : unmanaged
        {
            Memory<T> memory = _MemoryManager.Slice(memoryBlock.Index, memoryBlock.Length).Cast<byte, T>();
            IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>(this, memoryBlock.Index, memory);

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
                    uint newIndex = before.Value.Index;
                    uint newLength = before.Value.Length + current.Value.Length;
                    current.Value = current.Value with { Index = newIndex, Length = newLength };
                    _MemoryMap.Remove(before);
                }

                if (after?.Value.Owned is false)
                {
                    uint newLength = current.Value.Length + after.Value.Length;
                    current.Value = current.Value with { Length = newLength };
                    _MemoryMap.Remove(after);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LinkedListNode<MemoryBlock> GetMemoryBlockAtIndex(uint index)
        {
            LinkedListNode<MemoryBlock>? current = _MemoryMap.First;
            if (current is null) ThrowHelper.ThrowInvalidOperationException("Memory pool is in invalid state.");

            do
            {
                if (current!.Value.Index == index) return current;
            } while ((current = current?.Next) is not null);

            throw new InsufficientMemoryException("No memory block starts at index.");
        }
    }
}
