#region

using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class VertexArrayObject<TVertex> : IDisposable
        where TVertex : unmanaged
    {
        private readonly GL _GL;

        private bool _Disposed;
        private IVertexAttribute[] _VertexAttributes;

        private uint Handle { get; }
        public IReadOnlyCollection<IVertexAttribute> VertexAttributes => _VertexAttributes;

        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo)
        {
            _GL = gl;
            _VertexAttributes = new IVertexAttribute[0];

            Handle = _GL.CreateVertexArray();

            _GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, 0, (uint)sizeof(TVertex));
        }

        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo, BufferObject<uint> ebo)
        {
            _GL = gl;
            _VertexAttributes = new IVertexAttribute[0];

            Handle = _GL.CreateVertexArray();

            _GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, 0, (uint)sizeof(TVertex));
            _GL.VertexArrayElementBuffer(Handle, ebo.Handle);
        }

        public void AllocateVertexAttribute(IVertexAttribute vertexAttribute)
        {
            if (_VertexAttributes.Length < vertexAttribute.Index)
            {
                IVertexAttribute[] resized = new IVertexAttribute[vertexAttribute.Index + 1];
                Array.Copy(_VertexAttributes, 0, resized, 0, _VertexAttributes.Length);
                _VertexAttributes = resized;
            }

            _VertexAttributes[vertexAttribute.Index] = vertexAttribute;
        }

        public void AllocateVertexAttributes(IVertexAttribute[] vertexAttributes) => _VertexAttributes = vertexAttributes;

        public void CommitVertexAttributes()
        {
            for (uint index = 0; index < _VertexAttributes.Length; index++)
            {
                _GL.EnableVertexArrayAttrib(Handle, index);
                _VertexAttributes[index].Commit(_GL, Handle);
                _GL.VertexArrayAttribBinding(Handle, index, 0u);
            }
        }

        public void Bind() => _GL.BindVertexArray(Handle);
        public void Unbind() => _GL.BindVertexArray(0);

        public void Dispose()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());

            _GL.DeleteVertexArray(Handle);
            _Disposed = true;
        }
    }
}
