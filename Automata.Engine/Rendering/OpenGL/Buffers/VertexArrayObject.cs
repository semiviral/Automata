#region

using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class VertexArrayObject<TVertex> : OpenGLObject, IDisposable
        where TVertex : unmanaged
    {
        private bool _Disposed;
        private IVertexAttribute[] _VertexAttributes;

        public IReadOnlyCollection<IVertexAttribute> VertexAttributes => _VertexAttributes;

        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo) : base(gl)
        {
            _VertexAttributes = new IVertexAttribute[0];

            Handle = GL.CreateVertexArray();

            GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, 0, (uint)sizeof(TVertex));
        }

        public unsafe VertexArrayObject(GL gl, BufferObject<TVertex> vbo, BufferObject<uint> ebo) : base(gl)
        {
            _VertexAttributes = new IVertexAttribute[0];

            Handle = GL.CreateVertexArray();

            GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, 0, (uint)sizeof(TVertex));
            GL.VertexArrayElementBuffer(Handle, ebo.Handle);
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
                GL.EnableVertexArrayAttrib(Handle, index);
                _VertexAttributes[index].Commit(GL, Handle);
                GL.VertexArrayAttribBinding(Handle, index, 0u);
            }
        }

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        public void Dispose()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());

            GL.DeleteVertexArray(Handle);
            _Disposed = true;
        }
    }
}
