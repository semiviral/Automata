using System;
using System.Collections.Generic;
using Serilog;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class UniformBuffer : OpenGLObject, IDisposable
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
                {
                    Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(UniformBuffer),
                        "Offset is not aligned to a multiple of 16. This may be an error."));
                }

                if (!_Offsets.ContainsKey(uniform)) _Offsets.Add(uniform, value);
                else _Offsets[uniform] = value;
            }
        }

        public uint BindingIndex { get; }
        public uint Size { get; }

        public UniformBuffer(GL gl, uint bindingIndex, uint size) : base(gl)
        {
            if (size > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero and less than 16KB.");

            _Offsets = new Dictionary<string, int>();

            BindingIndex = bindingIndex;
            Size = size;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, Size, Span<byte>.Empty, (uint)_STORAGE_FLAGS);
        }

        public unsafe void Write<T>(int offset, T data) where T : unmanaged
        {
            if ((offset % 16) != 0)
            {
                Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(UniformBuffer),
                    "Offset is not aligned to a multiple of 16. This may be an error."));
            }

            GL.NamedBufferSubData(Handle, offset, (uint)sizeof(T), ref data);
        }

        public unsafe void Write<T>(string uniform, T data) where T : unmanaged =>
            GL.NamedBufferSubData(Handle, _Offsets[uniform], (uint)sizeof(T), ref data);

        public unsafe void Write<T>(int offset, Span<T> data) where T : unmanaged
        {
            if ((offset % 16) != 0)
            {
                Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(UniformBuffer),
                    "Offset is not aligned to a multiple of 16. This may be an error."));
            }

            GL.NamedBufferSubData(Handle, offset, (uint)(sizeof(T) * data.Length), ref data[0]);
        }

        public void Bind() => GL.BindBufferBase(BufferTargetARB.UniformBuffer, BindingIndex, Handle);
        public void Bind(int offset, uint size) => GL.BindBufferRange(BufferTargetARB.UniformBuffer, BindingIndex, Handle, offset, size);

        public void Dispose() => GL.DeleteBuffer(Handle);
    }
}
