using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class UniformBuffer
    {
        private readonly GL _GL;

        public uint Handle { get; }
        public string Name { get; }
        public uint BindingIndex { get; }

        public UniformBuffer(GL gl, string name, uint bindingIndex)
        {
            _GL = gl;
            Name = name;
            BindingIndex = bindingIndex;
            Handle = _GL.CreateBuffer();

            _GL.NamedBufferData(Handle, 512, Span<byte>.Empty, VertexBufferObjectUsage.StaticDraw);
        }

        public unsafe void Write<T>(int offset, T data) where T: unmanaged => _GL.NamedBufferSubData(Handle, offset, (uint)sizeof(T), ref data);

        public void Bind() => _GL.BindBufferBase(GLEnum.StaticDraw, BindingIndex, Handle);
        public void Bind(int offset, uint size) => _GL.BindBufferRange(GLEnum.StaticDraw, BindingIndex, Handle, offset, size);
    }
}
