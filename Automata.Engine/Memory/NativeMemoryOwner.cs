using System;
using System.Buffers;

namespace Automata.Engine.Memory
{
    /// <summary>
    ///     Used internally to properly track and dispose of rented memory blocks.
    /// </summary>
    /// <typeparam name="T">Unmanaged type of the <see cref="IMemoryOwner{T}" />.</typeparam>
    internal sealed class NativeMemoryOwner<T> : IMemoryOwner<T> where T : unmanaged
    {
        private readonly NativeMemoryPool _NativeMemoryPool;

        internal nuint Index { get; }
        public Memory<T> Memory { get; private set; }

        internal NativeMemoryOwner(NativeMemoryPool nativeMemoryPool, nuint index, Memory<T> memory)
        {
            _NativeMemoryPool = nativeMemoryPool;

            Index = index;
            Memory = memory;
        }

        public void Dispose()
        {
            if (Memory.IsEmpty)
            {
                return;
            }

            _NativeMemoryPool.Return(this);
            Memory = Memory<T>.Empty;
        }
    }
}
