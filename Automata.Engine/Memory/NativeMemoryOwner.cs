using System;
using System.Buffers;

namespace Automata.Engine.Memory
{
    /// <summary>
    ///     Used internally to properly track and dispose of rented memory blocks.
    /// </summary>
    /// <typeparam name="T">Unmanaged type of the <see cref="IMemoryOwner{T}"/>.</typeparam>
    internal sealed record NativeMemoryOwner<T> : IMemoryOwner<T> where T : unmanaged
    {
        private readonly NativeMemoryPool _NativeMemoryPool;

        internal nuint Index { get; }
        public Memory<T> Memory { get; }

        internal NativeMemoryOwner(NativeMemoryPool nativeMemoryPool, nuint index, Memory<T> memory)
        {
            _NativeMemoryPool = nativeMemoryPool;

            Index = index;
            Memory = memory;
        }

        public void Dispose() => _NativeMemoryPool.Return(this);
    }
}
