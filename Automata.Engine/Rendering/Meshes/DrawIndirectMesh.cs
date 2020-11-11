using System;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class DrawIndirectMesh : IMesh
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }
        public uint IndexesLength { get; }
        public uint IndexesByteLength { get; }

        public BufferObject<DrawElementsIndirectCommand> DrawCommandBuffer { get; }
        public BufferObject<byte> DataBuffer { get; }
        public VertexArrayObject<byte> VertexArrayObject { get; }

        public DrawIndirectMesh()
        {
            ID = new Guid();
            DrawCommandBuffer = new BufferObject<DrawElementsIndirectCommand>(GLAPI.Instance.GL);
            DataBuffer = new BufferObject<byte>(GLAPI.Instance.GL);
            VertexArrayObject = new VertexArrayObject<byte>(GLAPI.Instance.GL, DataBuffer, sizeof(uint) * 6, DataBuffer);
        }

        public void Bind()
        {
            DrawCommandBuffer.Bind(BufferTargetARB.DrawIndirectBuffer);
            VertexArrayObject.Bind();
        }

        public void Unbind()
        {
            GLAPI.UnbindBuffer(BufferTargetARB.DrawIndirectBuffer);
            GLAPI.UnbindVertexArray();
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
