using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record BufferMemory<T> : IDisposable where T : unmanaged
    {
        public IMemoryOwner<T> MemoryOwner { get; }
        public nuint Index { get; }

        public uint Count => (uint)MemoryOwner.Memory.Length;

        public BufferMemory(BufferAllocator allocator, nuint alignment, ReadOnlySpan<T> data)
        {
            MemoryOwner = allocator.Rent<T>(data.Length, alignment, out nuint index);
            data.CopyTo(MemoryOwner.Memory.Span);
            Index = index;
        }

        public BufferMemory(BufferAllocator allocator, nuint alignment, nuint size)
        {
            MemoryOwner = allocator.Rent<T>((int)size, alignment, out nuint index);
            Index = index;
        }


        #region IDisposable

        public void Dispose() => MemoryOwner.Dispose();

        #endregion
    }
}
