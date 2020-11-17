using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class MultiDrawElementsIndirectCommandBuffer : OpenGLObject
    {
        public uint Count { get; private set; }

        public MultiDrawElementsIndirectCommandBuffer(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public unsafe void AllocateCommands(Span<DrawElementsIndirectCommand> commands)
        {
            Count = (uint)commands.Length;
            GL.NamedBufferData(Handle, Count * (uint)sizeof(DrawElementsIndirectCommand), commands, VertexBufferObjectUsage.StaticDraw);
        }


        #region Binding

        public void Bind() => GL.BindBuffer(BufferTargetARB.DrawIndirectBuffer, Handle);

        #endregion


        #region IDisposable

        protected override void DisposeInternal() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
