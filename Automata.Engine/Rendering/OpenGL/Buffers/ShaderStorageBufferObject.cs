using System;
using System.Collections.Generic;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class ShaderStorageBufferObject : OpenGLObject, IDisposable
    {
        private const MapBufferAccessMask _MAPPING_FLAGS = MapBufferAccessMask.MapWriteBit;
        private const BufferStorageMask _STORAGE_FLAGS = BufferStorageMask.DynamicStorageBit | (BufferStorageMask)_MAPPING_FLAGS;

        private readonly Dictionary<string, int> _Offsets;

        public int this[string uniform]
        {
            get => _Offsets[uniform];
            init
            {
                if ((value % 16) != 0)
                    Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(UniformBufferObject),
                        "Offset is not aligned to a multiple of 16. This may be an error."));

                if (!_Offsets.ContainsKey(uniform)) _Offsets.Add(uniform, value);
                else _Offsets[uniform] = value;
            }
        }

        public uint BindingIndex { get; }
        public uint Size { get; }

        public ShaderStorageBufferObject(GL gl, uint bindingIndex, uint size) : base(gl)
        {
            if (size > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero and less than 16KB.");

            _Offsets = new Dictionary<string, int>();

            Handle = GL.CreateBuffer();
            BindingIndex = bindingIndex;
            Size = size;

            GL.NamedBufferStorage(Handle, Size, Span<byte>.Empty, (uint)_STORAGE_FLAGS);
        }

        public void Bind() => GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, BindingIndex, Handle);

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
            GC.SuppressFinalize(this);
        }
    }
}
