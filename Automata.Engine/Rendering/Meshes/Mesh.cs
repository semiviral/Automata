#region

using System;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TVertex> : IMesh where TVertex : unmanaged
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }

        public BufferObject<TVertex> VertexesBufferObject { get; }
        public BufferObject<uint> IndexesBufferObject { get; }
        public VertexArrayObject<TVertex> VertexArrayObject { get; }

        public uint IndexesLength => IndexesBufferObject.Length;
        public uint IndexesByteLength => IndexesBufferObject.ByteLength;

        public Mesh(GL gl, Layer layer = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            VertexesBufferObject = new BufferObject<TVertex>(gl);
            IndexesBufferObject = new BufferObject<uint>(gl);
            VertexArrayObject = new VertexArrayObject<TVertex>(gl, VertexesBufferObject, IndexesBufferObject);
        }

        public void Bind() => VertexArrayObject.Bind();
        public void Unbind() => VertexArrayObject.Unbind();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            VertexesBufferObject.Dispose();
            IndexesBufferObject.Dispose();
            VertexArrayObject.Dispose();
        }
    }
}
