using System;
using Automata.Engine.Rendering.OpenGL.Buffers;

namespace Automata.Engine.Rendering.OpenGL
{
    public sealed record AllocationWrapper : IDisposable
    {
        public BufferArrayMemory IndexesArrayMemory { get; }
        public BufferArrayMemory VertexArrayMemory { get; }

        public AllocationWrapper(BufferArrayMemory indexesArrayMemory, BufferArrayMemory vertexArrayMemory)
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
