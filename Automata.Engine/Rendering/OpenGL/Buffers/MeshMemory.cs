using System;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public sealed record MeshMemory<TIndex, TVertex> : IDisposable
        where TIndex : unmanaged
        where TVertex : unmanaged
    {
        public BufferMemory<TIndex> IndexesMemory { get; }
        public BufferMemory<TVertex> VertexMemory { get; }

        public MeshMemory(BufferMemory<TIndex> indexesMemory, BufferMemory<TVertex> vertexMemory)
        {
            IndexesMemory = indexesMemory;
            VertexMemory = vertexMemory;
        }


        #region IDisposable

        public void Dispose()
        {
            IndexesMemory.Dispose();
            VertexMemory.Dispose();
        }

        #endregion
    }
}
