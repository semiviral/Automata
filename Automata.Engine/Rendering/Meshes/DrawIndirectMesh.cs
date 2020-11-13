using System;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class DrawIndirectMesh : IMesh
    {
        uint IMesh.IndexesLength { get; }
        uint IMesh.IndexesByteLength { get; }

        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }

        public BufferObject<DrawElementsIndirectCommand> DrawCommandBuffer { get; }
        public BufferObject<byte> DataBuffer { get; }
        public VertexArrayObject<byte> VertexArrayObject { get; }

        public DrawIndirectMesh(GL gl)
        {
            ID = new Guid();
            DrawCommandBuffer = new BufferObject<DrawElementsIndirectCommand>(gl);
            DataBuffer = new BufferObject<byte>(gl);
            VertexArrayObject = new VertexArrayObject<byte>(gl, DataBuffer, sizeof(uint) * 6, DataBuffer);
        }

        public void Bind()
        {
            DrawCommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);
            VertexArrayObject.Bind();
        }

        public void Unbind()
        {
            DrawCommandBuffer.Unbind(BufferTargetARB.DrawIndirectBuffer);
            VertexArrayObject.Unbind();
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
