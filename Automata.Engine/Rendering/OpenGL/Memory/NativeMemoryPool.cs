using System.Buffers;
using System.Runtime.InteropServices;
using Automata.Engine.Collections;

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    public unsafe class NativeMemoryPool
    {
        private sealed record MemoryBlock
        {
            public uint Index { get; }
            public uint Length { get; }
            public bool Owned { get; init; }

            public MemoryBlock(uint index, uint length, bool owned) => (Index, Length, Owned) = (index, length, owned);
        }

        private readonly NativeMemoryManager<byte> _MemoryManager;
        private readonly MemoryList<MemoryBlock> _MemoryMap;

        public NativeMemoryPool(byte* nativePointer, uint length)
        {
            _MemoryManager = new NativeMemoryManager<byte>(nativePointer, length);

            _MemoryMap = new MemoryList<MemoryBlock>(8)
            {
                new MemoryBlock(0u, length, false)
            };
        }

        public void Rent<T>(uint length) where T : unmanaged
        {
            for (int index = 0; index < _MemoryMap.Count; index++)
            {
                MemoryBlock memoryBlock = _MemoryMap[index];

                if (memoryBlock.Length == length)
                {
                    _MemoryMap[index] = memoryBlock = memoryBlock with { Owned = true };
                    Memory<T> memory = _MemoryManager.Slice(memoryBlock.Index, memoryBlock.Length).
                    IMemoryOwner<T> memoryOwner = new NativeMemoryOwner<T>()
                }
                else if (memoryBlock.Length < length) { }
            }

            ThrowHelper.ThrowInvalidOperationException("Memory pool out of memory.");
        }
    }
}
