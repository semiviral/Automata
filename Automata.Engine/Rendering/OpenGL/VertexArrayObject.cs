using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
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

        public void Finalize(BufferObject vbo, BufferObject? ebo, int vertexOffset)
        {
            Dictionary<uint, uint> strides = new Dictionary<uint, uint>();

            foreach (IVertexAttribute vertexAttribute in _VertexAttributes.OrderBy(attribute => attribute.BindingIndex))
            {
                vertexAttribute.Commit(GL, Handle);
                GL.EnableVertexArrayAttrib(Handle, vertexAttribute.Index);
                GL.VertexArrayAttribBinding(Handle, vertexAttribute.Index, vertexAttribute.BindingIndex);
                if (vertexAttribute.Divisor > 0u) GL.VertexArrayBindingDivisor(Handle, vertexAttribute.BindingIndex, vertexAttribute.Divisor);

                if (strides.ContainsKey(vertexAttribute.BindingIndex)) strides[vertexAttribute.BindingIndex] += vertexAttribute.Stride;
                else strides.Add(vertexAttribute.BindingIndex, vertexAttribute.Stride);
            }

            foreach ((uint bindingIndex, uint stride) in strides) GL.VertexArrayVertexBuffer(Handle, bindingIndex, vbo.Handle, vertexOffset, stride);
            if (ebo is not null) GL.VertexArrayElementBuffer(Handle, ebo.Handle);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VertexArrayObject),
                $"Finalized (0x{Handle:x}): attributes {_VertexAttributes.Count}\r\n\t{string.Join(",\r\n\t", _VertexAttributes)}"));
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
