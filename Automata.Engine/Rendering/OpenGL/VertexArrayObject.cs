using System;
using System.Collections.Generic;
using Automata.Engine.Collections;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL
{
    public class VertexArrayObject : OpenGLObject
    {
        private sealed class VertexBufferObjectBinding
        {
            public uint Handle { get; }
            public nint VertexOffset { get; }
            public uint Divisor { get; }
            public uint Stride { get; set; }

            public VertexBufferObjectBinding(uint handle, nint vertexOffset = 0, uint divisor = 0u)
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

        public void AllocateVertexBufferBinding(uint bindingIndex, OpenGLObject buffer, int vertexOffset = 0, uint divisor = 0u)
        {
            if (_VertexBufferObjectBindings.ContainsKey(bindingIndex))
            {
                _VertexBufferObjectBindings[bindingIndex] = new VertexBufferObjectBinding(buffer.Handle, vertexOffset, divisor);
            }
            else
            {
                _VertexBufferObjectBindings.Add(bindingIndex, new VertexBufferObjectBinding(buffer.Handle, vertexOffset, divisor));
            }

            Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                $"Allocated new VBO binding: Handle 0x{buffer.Handle:x}, BindingIndex {bindingIndex}, VertexOffset {vertexOffset}"));
        }

        #endregion


        #region Finalization

        public void Finalize(OpenGLObject? ebo)
        {
            // calculate strides for new VBO bindings
            Dictionary<uint, uint> strides = new Dictionary<uint, uint>();

            foreach (IVertexAttribute vertex_attribute in _VertexAttributes)
            {
                GL.EnableVertexArrayAttrib(Handle, vertex_attribute.Index);
                vertex_attribute.CommitFormat(GL, Handle);
                GL.VertexArrayAttribBinding(Handle, vertex_attribute.Index, vertex_attribute.BindingIndex);

                // set or add stride by binding index of vertex attribute
                if (strides.ContainsKey(vertex_attribute.BindingIndex))
                {
                    strides[vertex_attribute.BindingIndex] += vertex_attribute.Stride;
                }
                else
                {
                    strides.Add(vertex_attribute.BindingIndex, vertex_attribute.Stride);
                }

                Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                    $"Committed vertex attribute: {vertex_attribute}"));
            }

            // commit VBO bindings
            foreach ((uint binding_index, VertexBufferObjectBinding binding) in _VertexBufferObjectBindings)
            {
                // reset strides to ensure we update with new vertex attrib formats
                binding.Stride = strides.TryGetValue(binding_index, out uint stride) ? stride : 0u;

                // bind given VBO binding
                GL.VertexArrayVertexBuffer(Handle, binding_index, binding.Handle, binding.VertexOffset, binding.Stride);

                if (binding.Divisor is not 0u)
                {
                    GL.VertexArrayBindingDivisor(Handle, binding_index, binding.Divisor);
                }

                Log.Verbose(String.Format(FormatHelper.DEFAULT_LOGGING, $"{nameof(VertexArrayObject)} 0x{Handle}",
                    $"Bound VBO: Handle 0x{binding.Handle:x}, BindingIndex {binding_index}, Stride {stride}, VertexOffset {binding.VertexOffset}, Divisor {binding.Divisor}"));
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

        protected override void CleanupManagedResources() => _VertexAttributes.Dispose();
        protected override void CleanupNativeResources() => GL.DeleteVertexArray(Handle);

        #endregion
    }
}
