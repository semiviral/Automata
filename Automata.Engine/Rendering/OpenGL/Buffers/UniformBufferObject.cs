using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class UniformBufferObject : OpenGLObject
    {
        private const int _ALIGNMENT = 16;

        private const BufferStorageMask _STORAGE_FLAGS = BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit;

        private readonly Dictionary<string, int> _Offsets;

        public int this[string uniform]
        {
            get => _Offsets[uniform];
            init
            {
                Debug.Assert((value % _ALIGNMENT) == 0, "Offset is not aligned to a multiple of 16. This may be an error.");

                if (!_Offsets.ContainsKey(uniform))
                {
                    _Offsets.Add(uniform, value);
                }
                else
                {
                    _Offsets[uniform] = value;
                }
            }
        }

        /// <summary>
        ///     Uniform index buffer is bound to.
        /// </summary>
        public uint BindingIndex { get; }

        public uint Size { get; }

        public UniformBufferObject(GL gl, uint bindingIndex, uint size, BufferStorageMask bufferStorageMask = _STORAGE_FLAGS) : base(gl)
        {
            if (size > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero and less than 16KB.");
            }

            _Offsets = new Dictionary<string, int>();

            Handle = GL.CreateBuffer();
            BindingIndex = bindingIndex;
            Size = size;

            GL.NamedBufferStorage(Handle, Size, Span<byte>.Empty, (uint)bufferStorageMask);
        }

        public unsafe void Write<T>(int offset, T data) where T : unmanaged
        {
            Debug.Assert((offset % _ALIGNMENT) == 0, "Offset is not aligned to a multiple of 16. This may be an error.");

            uint length = Size - (uint)offset;
            void* pointer = GL.MapNamedBufferRange(Handle, offset, length, (uint)(MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateRangeBit));
            Unsafe.Write(pointer, data);
            GL.UnmapNamedBuffer(Handle);
        }

        public unsafe void Write<T>(int offset, Span<T> data) where T : unmanaged
        {
            Debug.Assert((offset % _ALIGNMENT) == 0, "Offset is not aligned to a multiple of 16. This may be an error.");

            uint length = Size - (uint)offset;
            void* pointer = GL.MapNamedBufferRange(Handle, offset, length, (uint)(MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateRangeBit));
            MemoryMarshal.AsBytes(data).CopyTo(new Span<byte>(pointer, (int)length));
            GL.UnmapNamedBuffer(Handle);
        }

        public unsafe void Write<T>(string uniform, T data) where T : unmanaged =>
            GL.NamedBufferSubData(Handle, _Offsets[uniform], (uint)sizeof(T), ref data);


        #region Binding

        public void Bind() => GL.BindBufferBase(BufferTargetARB.UniformBuffer, BindingIndex, Handle);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
