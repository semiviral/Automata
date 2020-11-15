#region

using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class VertexArrayObject : OpenGLObject, IDisposable
    {
        private bool _Disposed;
        private IVertexAttribute[] _VertexAttributes;

        public IReadOnlyCollection<IVertexAttribute> VertexAttributes => _VertexAttributes;

        public VertexArrayObject(GL gl, BufferObject vbo, uint vertexStride, int vertexOffset) : base(gl)
        {
            _VertexAttributes = Array.Empty<IVertexAttribute>();

            Handle = GL.CreateVertexArray();

            AssignVertexArrayVertexBuffer(vbo, vertexStride, vertexOffset);
        }

        public VertexArrayObject(GL gl, BufferObject vbo, BufferObject ebo, uint vertexStride, int vertexOffset) : this(gl, vbo, vertexStride, vertexOffset) =>
            AssignVertexArrayElementBuffer(ebo);

        public void AssignVertexArrayVertexBuffer(BufferObject vbo, uint vertexStride, int vertexOffset) =>
            GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, vertexOffset, vertexStride);

        public unsafe void AssignVertexArrayVertexBuffer<TVertex>(BufferObject vbo, int vertexOffset) where TVertex : unmanaged =>
            GL.VertexArrayVertexBuffer(Handle, 0, vbo.Handle, vertexOffset, (uint)sizeof(TVertex));

        public void AssignVertexArrayElementBuffer(BufferObject ebo) => GL.VertexArrayElementBuffer(Handle, ebo.Handle);


        #region Vertex Attributes

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

        #endregion


        #region Binding

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());

            GL.DeleteVertexArray(Handle);

            _Disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
