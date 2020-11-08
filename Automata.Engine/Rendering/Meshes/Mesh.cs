#region

using System;
using Automata.Engine.Rendering.OpenGL;

#endregion


namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<TVertex> : IMesh where TVertex : unmanaged
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }

        public BufferObject<TVertex> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<TVertex> VertexArrayObject { get; }

        public uint IndexesLength => IndexesBuffer.Length;
        public uint IndexesByteLength => IndexesBuffer.ByteLength;

        public Mesh(Layer layer = Layer.Layer0)
        {
            ID = Guid.NewGuid();
            Visible = true;
            Layer = layer;
            VertexesBuffer = new BufferObject<TVertex>(GLAPI.Instance.GL);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL);
            VertexArrayObject = new VertexArrayObject<TVertex>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }

        public void Bind() => VertexArrayObject.Bind();
        public void Unbind() => VertexArrayObject.Unbind();

        public void Dispose()
        {
            VertexesBuffer.Dispose();
            IndexesBuffer.Dispose();
            VertexArrayObject.Dispose();
        }
    }
}
