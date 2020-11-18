using System;
using Automata.Engine.Rendering.OpenGL.Buffers;

namespace Automata.Engine.Rendering.OpenGL
{
    public sealed record AllocationWrapper<TIndex, TVertex> : IDisposable
    where TIndex : unmanaged
    where TVertex : unmanaged
    {
        public BufferArrayMemory<TIndex> IndexesArrayMemory { get; }
        public BufferArrayMemory<TVertex> VertexArrayMemory { get; }

        public AllocationWrapper(BufferArrayMemory<TIndex> indexesArrayMemory, BufferArrayMemory<TVertex> vertexArrayMemory)
        {
            IndexesArrayMemory = indexesArrayMemory;
            VertexArrayMemory = vertexArrayMemory;
        }


        #region IDisposable

        public void Dispose()
        {
            IndexesArrayMemory.Dispose();
            VertexArrayMemory.Dispose();
        }

        #endregion
    }
}
