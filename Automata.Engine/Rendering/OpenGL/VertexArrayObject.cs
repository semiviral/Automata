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
        private sealed class VertexBufferObjectBinding
        {
            public uint Handle { get; }
            public uint Stride { get; set; }
            public int VertexOffset { get; }

            public VertexBufferObjectBinding(uint handle, int vertexOffset)
            {
                Handle = handle;
                VertexOffset = vertexOffset;
            }
        }

        private readonly NonAllocatingList<IVertexAttribute> _VertexAttributes;
        private readonly Dictionary<uint, VertexBufferObjectBinding> _VertexBufferObjectBindings;

        public VertexArrayObject(GL gl) : base(gl)
        {
            _VertexAttributes = new NonAllocatingList<IVertexAttribute>();
            _VertexBufferObjectBindings = new Dictionary<uint, VertexBufferObjectBinding>();

            Handle = GL.CreateVertexArray();
        }


        #region Vertex Attributes

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] vertexAttributes)
        {
            if (replace) _VertexAttributes.Clear();
            _VertexAttributes.AddRange(vertexAttributes);
        }

        public void BindVertexBuffer(uint bindingIndex, BufferObject vbo, int vertexOffset)
        {
            if (_VertexBufferObjectBindings.ContainsKey(bindingIndex))
                _VertexBufferObjectBindings[bindingIndex] = new VertexBufferObjectBinding(vbo.Handle, vertexOffset);
            else _VertexBufferObjectBindings.Add(bindingIndex, new VertexBufferObjectBinding(vbo.Handle, vertexOffset));
        }

        public void Finalize(BufferObject? ebo)
        {
            foreach (IVertexAttribute vertexAttribute in _VertexAttributes.OrderBy(attribute => attribute.BindingIndex))
            {

                vertexAttribute.CommitFormat(GL, Handle);
                GL.EnableVertexArrayAttrib(Handle, vertexAttribute.Index);
                GL.VertexArrayAttribBinding(Handle, vertexAttribute.Index, vertexAttribute.BindingIndex);
                if (vertexAttribute.Divisor > 0u) GL.VertexArrayBindingDivisor(Handle, vertexAttribute.BindingIndex, vertexAttribute.Divisor);

                if (_VertexBufferObjectBindings.TryGetValue(vertexAttribute.BindingIndex, out VertexBufferObjectBinding? binding))
                    binding!.Stride += vertexAttribute.Stride;
            }

            foreach ((uint bindingIndex, VertexBufferObjectBinding binding) in _VertexBufferObjectBindings)
                GL.VertexArrayVertexBuffer(Handle, bindingIndex, binding.Handle, binding.VertexOffset, binding.Stride);

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

        protected override void DisposeInternal()
        {
            _VertexAttributes.Dispose();
            GL.DeleteVertexArray(Handle);
        }

        #endregion
    }
}
