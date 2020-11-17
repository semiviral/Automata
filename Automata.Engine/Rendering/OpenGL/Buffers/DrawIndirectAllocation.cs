using System;
using System.Buffers;
using Automata.Engine.Components;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class DrawIndirectAllocation : ComponentChangeable, IDisposable
    {
        public sealed record AllocationWrapper : IDisposable
        {
            public IMemoryOwner<uint> IndexesOwner { get; }
            public IMemoryOwner<byte> VertexesOwner { get; }
            public nuint IndexesIndex { get; }
            public nuint VertexesIndex { get; }
            public uint VertexCount { get; }

            public AllocationWrapper(IMemoryOwner<uint> indexesOwner, IMemoryOwner<byte> vertexesOwner, nuint indexesIndex, nuint vertexesIndex,
                uint vertexCount)
            {
                IndexesOwner = indexesOwner;
                VertexesOwner = vertexesOwner;
                IndexesIndex = indexesIndex;
                VertexesIndex = vertexesIndex;
                VertexCount = vertexCount;
            }

            public void Dispose()
            {
                IndexesOwner.Dispose();
                VertexesOwner.Dispose();
            }
        }

        private AllocationWrapper? _Allocation;

        public AllocationWrapper? Allocation
        {
            get => _Allocation;
            set
            {
                _Allocation = value;
                Changed = true;
            }
        }

        public void Dispose()
        {
            Allocation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
