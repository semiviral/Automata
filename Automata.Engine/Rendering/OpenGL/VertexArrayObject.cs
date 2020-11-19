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
            public int VertexOffset { get; }
            public uint Divisor { get; }
            public uint Stride { get; set; }

            public VertexBufferObjectBinding(uint handle, int vertexOffset = 0, uint divisor = 0u)
            {
                Handle = handle;
                VertexOffset = vertexOffset;
                Divisor = divisor;
            }
        }

        private readonly NonAllocatingList<IVertexAttribute> _VertexAttributes;
        private readonly Dictionary<uint, VertexBufferObjectBinding> _VertexBufferObjectBindings;

        public VertexArrayObject(GL gl) : base(gl)
        {
            _VertexAttributes = new NonAllocatingList<IVertexAttribute>();
            _VertexBufferObjectBindings = new Dictionary<uint, VertexBufferObjectBinding>();

            Handle = GL.CreateVertexArray();

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}", "Allocated new VAO."));
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

        public void BindVertexBuffer(uint bindingIndex, BufferObject vbo, int vertexOffset = 0, uint divisor = 0u)
        {
            if (_VertexBufferObjectBindings.ContainsKey(bindingIndex))
            {
                _VertexBufferObjectBindings[bindingIndex] = new VertexBufferObjectBinding(vbo.Handle, vertexOffset, divisor);
            }
            else
            {
                _VertexBufferObjectBindings.Add(bindingIndex, new VertexBufferObjectBinding(vbo.Handle, vertexOffset, divisor));
            }

            Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                $"Allocated new VBO binding: Handle 0x{vbo.Handle:x}, BindingIndex {bindingIndex}, VertexOffset {vertexOffset}"));
        }

        #endregion


        #region Finalization

        public void Finalize(BufferObject? ebo)
        {
            // calculate strides for new VBO bindings
            Dictionary<uint, uint> strides = new Dictionary<uint, uint>();

            foreach (IVertexAttribute vertexAttribute in _VertexAttributes)
            {
                GL.EnableVertexArrayAttrib(Handle, vertexAttribute.Index);
                vertexAttribute.CommitFormat(GL, Handle);
                GL.VertexArrayAttribBinding(Handle, vertexAttribute.Index, vertexAttribute.BindingIndex);

                // set or add stride by binding index of vertex attribute
                if (strides.ContainsKey(vertexAttribute.BindingIndex))
                {
                    strides[vertexAttribute.BindingIndex] += vertexAttribute.Stride;
                }
                else
                {
                    strides.Add(vertexAttribute.BindingIndex, vertexAttribute.Stride);
                }

                Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                    $"Committed vertex attribute: {vertexAttribute}"));
            }

            // commit VBO bindings
            foreach ((uint bindingIndex, VertexBufferObjectBinding binding) in _VertexBufferObjectBindings)
            {
                // reset strides to ensure we update with new vertex attrib formats
                binding.Stride = strides.TryGetValue(bindingIndex, out uint stride) ? stride : 0u;

                // bind given VBO binding
                GL.VertexArrayVertexBuffer(Handle, bindingIndex, binding.Handle, binding.VertexOffset, binding.Stride);

                if (binding.Divisor is not 0u)
                {
                    GL.VertexArrayBindingDivisor(Handle, bindingIndex, binding.Divisor);
                }

                Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                    $"Bound VBO: Handle 0x{binding.Handle:x}, BindingIndex {bindingIndex}, Stride {stride}, VertexOffset {binding.VertexOffset}, Divisor {binding.Divisor}"));
            }

            if (ebo is not null)
            {
                GL.VertexArrayElementBuffer(Handle, ebo.Handle);
            }
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
