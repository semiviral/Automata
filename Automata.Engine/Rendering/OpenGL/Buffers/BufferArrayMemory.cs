using System;
using System.Buffers;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record BufferArrayMemory : IDisposable
    {
        public IMemoryOwner<byte> MemoryOwner { get; }
        public nuint Index { get; }

        public uint Count => (uint)MemoryOwner.Memory.Length;

        public BufferArrayMemory(BufferAllocator allocator, nuint alignment, nuint length)
        {
            MemoryOwner = allocator.Rent<byte>((int)length, alignment, out nuint index);
            Index = index;
        }


        #region IDisposable

        public void Dispose() => MemoryOwner.Dispose();

        #endregion
    }
}
