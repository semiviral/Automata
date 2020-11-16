using System;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering
{
    public class VertexMesh<TVertex> : IMesh where TVertex : unmanaged
    {
        private readonly GL _GL;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; }

        public BufferObject<TVertex> BufferObject { get; }
        public VertexArrayObject<TVertex> VertexArrayObject { get; }

        public VertexMesh(GL gl)
        {
            _GL = gl;

            ID = Guid.NewGuid();
            Layer = Layer.Layer0;
            Visible = true;

            BufferObject = new BufferObject<TVertex>(gl);
            VertexArrayObject = new VertexArrayObject<TVertex>(gl, BufferObject, 0);
        }

        public void Draw()
        {
            VertexArrayObject.Bind();
            _GL.DrawArrays(PrimitiveType.Triangles, 0, BufferObject.DataLength);
        }

        public void Dispose()
        {
            BufferObject.Dispose();
            VertexArrayObject.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
