using Automata.Engine.Collections;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class VertexArrayObject : OpenGLObject
    {
        private readonly NonAllocatingList<IVertexAttribute> _VertexAttributes;

        public VertexArrayObject(GL gl) : base(gl)
        {
            _VertexAttributes = new NonAllocatingList<IVertexAttribute>();

            Handle = GL.CreateVertexArray();
        }


        #region Vertex Attributes

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] vertexAttributes)
        {
            if (replace) _VertexAttributes.Clear();
            _VertexAttributes.AddRange(vertexAttributes);
        }

        public void CommitVertexAttributes(BufferObject vbo, BufferObject? ebo, int vertexOffset)
        {
            uint stride = 0u;

            foreach (IVertexAttribute vertexAttribute in _VertexAttributes)
            {
                vertexAttribute.Commit(GL, Handle);
                GL.EnableVertexArrayAttrib(Handle, vertexAttribute.Index);
                GL.VertexArrayAttribBinding(Handle, vertexAttribute.Index, 0u);
                stride += vertexAttribute.Stride;
            }

            GL.VertexArrayVertexBuffer(Handle, 0u, vbo.Handle, vertexOffset, stride);
            if (ebo is not null) GL.VertexArrayElementBuffer(Handle, ebo.Handle);
        }

        #endregion


        #region Binding

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        #endregion


        #region IDisposable

        protected override void DisposeInternal() => GL.DeleteVertexArray(Handle);

        #endregion
    }
}
