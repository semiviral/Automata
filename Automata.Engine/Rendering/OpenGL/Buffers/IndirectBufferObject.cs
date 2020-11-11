using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class IndirectBufferObject : OpenGLObject
    {
        public uint Count { get; }

        public unsafe IndirectBufferObject(GL gl, uint count) : base(gl)
        {
            Handle = GL.CreateBuffer();
            Count = count;

            GL.NamedBufferStorage(Handle, Count * (uint)sizeof(DrawElementsIndirectCommand), Span<byte>.Empty, (uint)BufferStorageMask.DynamicStorageBit);
        }

        public unsafe void WriteCommand(uint index, DrawElementsIndirectCommand command) =>
            GL.NamedBufferSubData(Handle, (int)(index * (uint)sizeof(DrawElementsIndirectCommand)), (uint)sizeof(DrawElementsIndirectCommand), ref command);

        public unsafe void WriteCommands(Span<(uint, DrawElementsIndirectCommand)> commands)
        {
            foreach ((uint index, DrawElementsIndirectCommand command) in commands)
                GL.NamedBufferSubData(Handle, (int)(index * (uint)sizeof(DrawElementsIndirectCommand)), (uint)sizeof(DrawElementsIndirectCommand), &command);
        }
    }
}
