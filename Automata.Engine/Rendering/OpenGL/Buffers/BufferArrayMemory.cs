using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record BufferArrayMemory<T> : IDisposable where T : unmanaged
    {
        public IMemoryOwner<T> MemoryOwner { get; }
        public nuint Index { get; }

        public uint Count => (uint)MemoryOwner.Memory.Length;

        public BufferArrayMemory(BufferAllocator allocator, nuint alignment, ReadOnlySpan<T> data)
        {
            MemoryOwner = allocator.Rent<T>(data.Length, alignment, out nuint index);
            Index = index;
        }


        #region IDisposable

        public void Dispose() => MemoryOwner.Dispose();

        #endregion
    }
}
