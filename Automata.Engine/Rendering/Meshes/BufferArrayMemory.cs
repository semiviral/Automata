using System;
using System.Buffers;

namespace Automata.Engine.Rendering.Meshes
{
    public delegate IMemoryOwner<T> AllocationRenter<T>(int length, nuint alignment, out nuint index, bool clear = false) where T : unmanaged;

    public record BufferArrayMemory<T> : IDisposable where T : unmanaged
    {
        public IMemoryOwner<T> MemoryOwner { get; }
        public nuint Index { get; }
        public uint Count { get; }

        public BufferArrayMemory(AllocationRenter<T> renter, nuint alignment, ReadOnlySpan<T> data)
        {
            MemoryOwner = renter(data.Length, alignment, out nuint index);
            data.CopyTo(MemoryOwner.Memory.Span);
            Index = index;
            Count = (uint)data.Length;
        }

        public void Dispose()
        {
            MemoryOwner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
