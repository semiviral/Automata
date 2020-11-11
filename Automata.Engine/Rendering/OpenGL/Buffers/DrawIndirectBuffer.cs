using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class DrawIndirectBuffer : OpenGLObject
    {
        public uint Size { get; }

        public unsafe DrawIndirectBuffer(GL gl, uint size) : base(gl)
        {
            Handle = GL.CreateBuffer();
            Size = size;

            GL.NamedBufferStorage(Handle, Size, Span<byte>.Empty, (uint)BufferTargetARB.DrawIndirectBuffer);
            new VertexAttribute<uint>(2, 1, (uint)sizeof(DrawElementsIndirectCommand)).Commit(GL, Handle);
            GL.VertexArrayAttribBinding(Handle, 2u, 0u);
            GL.VertexArrayBindingDivisor(Handle, 2u, 1u);
            GL.EnableVertexArrayAttrib(Handle, 2u);
        }

        public unsafe void WriteCommand(DrawElementsIndirectCommand command, uint index) =>
            GL.NamedBufferSubData(Handle, (int)(index * (uint)sizeof(DrawElementsIndirectCommand)), (uint)sizeof(DrawElementsIndirectCommand), ref command);

        public unsafe void WriteCommands(Span<(uint, DrawElementsIndirectCommand)> commands)
        {
            foreach ((uint index, DrawElementsIndirectCommand command) in commands)
                GL.NamedBufferSubData(Handle, (int)(index * (uint)sizeof(DrawElementsIndirectCommand)), (uint)sizeof(DrawElementsIndirectCommand), &command);
        }
    }
}
