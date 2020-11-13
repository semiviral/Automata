using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Memory
{
    internal record NativeMemoryOwner<T> : IMemoryOwner<T> where T : unmanaged
    {
        private NativeMemoryPool _NativeMemoryPool { get; }

        public Memory<T> Memory { get; }

        internal NativeMemoryOwner(NativeMemoryPool nativeMemoryPool, Memory<T> memory)
        {
            _NativeMemoryPool = nativeMemoryPool;
            Memory = memory;
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
