using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    internal sealed record NativeMemoryOwner<T> : IMemoryOwner<T> where T : unmanaged
    {
        private readonly NativeMemoryPool _NativeMemoryPool;

        internal uint Index { get; }
        public Memory<T> Memory { get; }

        internal NativeMemoryOwner(NativeMemoryPool nativeMemoryPool, uint index, Memory<T> memory)
        {
            _NativeMemoryPool = nativeMemoryPool;

            Index = index;
            Memory = memory;
        }

        public void Dispose() => _NativeMemoryPool.Return(this);
    }
}
