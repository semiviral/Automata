using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class VertexArrayObject<TVertex> : VertexArrayObject where TVertex : unmanaged
    {
        public VertexArrayObject(GL gl) : base(gl) { }

        public void CommitVertexAttributes(BufferObject<TVertex> vbo, BufferObject<uint>? ebo, int vertexOffset) =>
            base.CommitVertexAttributes(vbo, ebo, vertexOffset);
    }
}
