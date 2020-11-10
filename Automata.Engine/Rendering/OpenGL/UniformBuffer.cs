using System;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class UniformBuffer : OpenGLObject
    {
        public string Name { get; }
        public uint BindingIndex { get; }

        public UniformBuffer(GL gl, string name, uint bindingIndex) : base(gl)
        {
            Name = name;
            BindingIndex = bindingIndex;
            Handle = GL.CreateBuffer();

            GL.NamedBufferData(Handle, 512, Span<byte>.Empty, VertexBufferObjectUsage.StaticDraw);
        }

        public unsafe void Write<T>(int offset, T data) where T : unmanaged
        {
            if ((offset % 16) != 0)
                Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(UniformBuffer),
                    "Offset is not aligned to a multiple of 16. This may be an error."));

            GL.NamedBufferSubData(Handle, offset, (uint)sizeof(T), ref data);
        }

        public void Bind() => GL.BindBufferBase(GLEnum.StaticDraw, BindingIndex, Handle);
        public void Bind(int offset, uint size) => GL.BindBufferRange(GLEnum.StaticDraw, BindingIndex, Handle, offset, size);
    }
}
