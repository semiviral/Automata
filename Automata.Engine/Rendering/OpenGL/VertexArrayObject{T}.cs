using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class VertexArrayObject<TVertex> : VertexArrayObject where TVertex : unmanaged
    {
        public VertexArrayObject(GL gl) : base(gl) { }

        public void CommitVertexAttributes(BufferObject<TVertex> vbo, BufferObject<uint>? ebo, int vertexOffset) =>
            Finalize(ebo);
    }
}
