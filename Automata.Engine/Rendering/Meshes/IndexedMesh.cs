#region

using System;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TVertex> : IMesh where TVertex : unmanaged
    {
        private readonly GL _GL;

        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; }

        public BufferObject<TVertex> VertexesBufferObject { get; }
        public BufferObject<uint> IndexesBufferObject { get; }
        public VertexArrayObject<TVertex> VertexArrayObject { get; }

        public Mesh(GL gl, Layer layer = Layer.Layer0)
        {
            _GL = gl;

            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            VertexesBufferObject = new BufferObject<TVertex>(gl);
            IndexesBufferObject = new BufferObject<uint>(gl);
            VertexArrayObject = new VertexArrayObject<TVertex>(gl, VertexesBufferObject, IndexesBufferObject);
        }

        public unsafe void Draw()
        {
            VertexArrayObject.Bind();

            _GL.DrawElements(PrimitiveType.Triangles, IndexesBufferObject.Length, DrawElementsType.UnsignedInt, (void*)null!);
        }

        public void Dispose()
        {
            VertexesBufferObject.Dispose();
            IndexesBufferObject.Dispose();
            VertexArrayObject.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
