#region

using System;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class QuadsMesh<TVertex> : IMesh where TVertex : unmanaged
    {
        private readonly GL _GL;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; set; }
        public BufferObject BufferObject { get; }
        public VertexArrayObject VertexArrayObject { get; }
        public uint IndexesCount { get; set; }

        public unsafe QuadsMesh(GL gl, Layer layer = Layer.Layer0)
        {
            _GL = gl;

            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            BufferObject = new BufferObject<Quad<TVertex>>(gl);
            VertexArrayObject = new VertexArrayObject(gl);
        }

        public unsafe void Draw()
        {
            VertexArrayObject.Bind();

            _GL.DrawElements(PrimitiveType.Triangles, IndexesCount, DrawElementsType.UnsignedInt, (void*)null!);
        }

        public void Dispose()
        {
            BufferObject.Dispose();
            VertexArrayObject.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
