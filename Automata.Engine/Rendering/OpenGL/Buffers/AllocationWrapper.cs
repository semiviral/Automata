using System;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record AllocationWrapper<TIndex, TVertex> : IDisposable
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        public BufferArrayMemory<TIndex> IndexesArrayMemory { get; }
        public BufferArrayMemory<TVertex> VertexArrayMemory { get; }

        public AllocationWrapper(BufferArrayMemory<TIndex> indexesArrayMemory, BufferArrayMemory<TVertex> vertexArrayMemory)
        {
            IndexesArrayMemory = indexesArrayMemory;
            VertexArrayMemory = vertexArrayMemory;
        }

        public void Dispose()
        {
            IndexesArrayMemory.Dispose();
            VertexArrayMemory.Dispose();
        }
    }
}
