using System;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record MeshArrayMemory<TIndex, TVertex> : IDisposable
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        public BufferArrayMemory<TIndex> IndexesArrayMemory { get; }
        public BufferArrayMemory<TVertex> VertexArrayMemory { get; }

        public MeshArrayMemory(BufferArrayMemory<TIndex> indexesArrayMemory, BufferArrayMemory<TVertex> vertexArrayMemory)
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
