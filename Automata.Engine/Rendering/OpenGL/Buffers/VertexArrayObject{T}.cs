using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class VertexArrayObject<TVertex> : VertexArrayObject, IDisposable where TVertex : unmanaged
    {
        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo, int vertexOffset) : base(gl, vbo, (uint)sizeof(TVertex), vertexOffset) { }

        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo, BufferObject<uint> ebo, int vertexOffset) : base(gl, vbo, ebo,
            (uint)sizeof(TVertex), vertexOffset) { }
    }
}
