using System;
using System.Collections.Generic;
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


        #region State

        public void AllocateVertexAttributes(bool replace, params IVertexAttribute[] vertexAttributes)
        {
            if (replace)
            {
                _VertexAttributes.Clear();
            }

            _VertexAttributes.AddRange(vertexAttributes);
        }

        public void BindVertexBuffer(uint bindingIndex, BufferObject vbo, int vertexOffset)
        {
            if (_VertexBufferObjectBindings.ContainsKey(bindingIndex))
            {
                _VertexBufferObjectBindings[bindingIndex] = new VertexBufferObjectBinding(vbo.Handle, vertexOffset);
            }
            else
            {
                _VertexBufferObjectBindings.Add(bindingIndex, new VertexBufferObjectBinding(vbo.Handle, vertexOffset));
            }

            Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, nameof(VertexArrayObject),
                $"Allocated new VBO binding (VAO 0x{Handle:x}): Handle 0x{vbo.Handle:x}, BindingIndex {bindingIndex}, VertexOffset {vertexOffset}"));
        }

        #endregion


        #region Finalization

        public void Finalize(BufferObject? ebo)
        {
            CommitBufferBindings(ebo);
            CommitVertexAttributes();

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VertexArrayObject),
                $"Finalized (VAO 0x{Handle:x}): {_VertexAttributes.Count} attributes\r\n\t{string.Join(",\r\n\t", _VertexAttributes)}"));
        }

        private void CommitBufferBindings(BufferObject? ebo)
        {
            // calculate strides for new VBO bindings
            Dictionary<uint, uint> strides = new Dictionary<uint, uint>();

            foreach (IVertexAttribute vertexAttribute in _VertexAttributes)
            {
                if (strides.ContainsKey(vertexAttribute.BindingIndex))
                {
                    strides[vertexAttribute.BindingIndex] += vertexAttribute.Stride;
                }
                else
                {
                    strides.Add(vertexAttribute.BindingIndex, vertexAttribute.Stride);
                }
            }

            // commit VBO bindings
            foreach ((uint bindingIndex, VertexBufferObjectBinding binding) in _VertexBufferObjectBindings)
            {
                // reset strides to ensure we update with new vertex attrib formats
                binding.Stride = strides.TryGetValue(bindingIndex, out uint stride) ? stride : 0u;

                // bind given VBO binding
                GL.VertexArrayVertexBuffer(Handle, bindingIndex, binding.Handle, binding.VertexOffset, binding.Stride);

                Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, nameof(VertexArrayObject),
                    $"Bound VBO (VAO 0x{Handle:x}): Handle 0x{binding.Handle:x}, BindingIndex {bindingIndex}, Stride {stride}, VertexOffset {binding.VertexOffset}"));
            }

            if (ebo is not null)
            {
                GL.VertexArrayElementBuffer(Handle, ebo.Handle);
            }
        }

        private void CommitVertexAttributes()
        {
            foreach (IVertexAttribute vertexAttribute in _VertexAttributes)
            {
                GL.EnableVertexArrayAttrib(Handle, vertexAttribute.Index);
                vertexAttribute.CommitFormat(GL, Handle);
                GL.VertexArrayAttribBinding(Handle, vertexAttribute.Index, vertexAttribute.BindingIndex);

                if (vertexAttribute.Divisor > 0u)
                {
                    GL.VertexArrayBindingDivisor(Handle, vertexAttribute.BindingIndex, vertexAttribute.Divisor);
                }
            }
        }

        #endregion


        #region Binding

        public void Bind()
        {
            // foreach ((_, VertexBufferObjectBinding binding) in _VertexBufferObjectBindings)
            //     GL.BindBuffer(BufferTargetARB.ArrayBuffer, binding.Handle);

            GL.BindVertexArray(Handle);
        }

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
