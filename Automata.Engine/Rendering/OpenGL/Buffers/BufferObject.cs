using System;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class BufferObject : OpenGLObject
    {
        private const MapBufferAccessMask _WRITE_ALL_ACCESS_MASK = MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateRangeBit;

        public nuint Length { get; protected set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public BufferObject(GL gl, void* data, nuint length, BufferStorageMask flags) : base(gl)
        {
            Length = length;
            GL.NamedBufferStorage(Handle, Length, data, (uint)flags);
        }

        public BufferObject(GL gl, void* data, nuint length, BufferDraw usage) : base(gl)
        {
            Length = length;
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)usage);
        }

        public BufferObject(GL gl, nuint length, BufferStorageMask flags) : this(gl, (void*)null!, length, flags) { }
        public BufferObject(GL gl, nuint length, BufferDraw usage) : this(gl, (void*)null!, length, usage) { }


        #region Pinning

        public void* Pin(MapBufferAccessMask flags) => GL.MapNamedBufferRange(Handle, (nint)0, Length, (uint)flags);
        public T* Pin<T>(MapBufferAccessMask flags) where T : unmanaged => (T*)Pin(flags);
        public void* PinRange(nint offset, nuint length, MapBufferAccessMask flags) => GL.MapNamedBufferRange(Handle, offset, length, (uint)flags);
        public T* PinRange<T>(nint offset, nuint length, MapBufferAccessMask flags) where T : unmanaged => (T*)PinRange(offset, length * (nuint)sizeof(T), flags);
        public void Unpin() => GL.UnmapNamedBuffer(Handle);

        #endregion


        #region Data

        public void SetData(void* data, nuint length, BufferDraw bufferDraw)
        {
            Length = length;
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SubData(nint offset, nuint length, void* data)
        {
            void* pointer = PinRange(offset, length, _WRITE_ALL_ACCESS_MASK);
            Buffer.MemoryCopy(data, pointer, length, length);
            Unpin();
        }

        public void SetData<T>(ReadOnlySpan<T> data, BufferDraw bufferDraw) where T : unmanaged
        {
            Length = (nuint)(data.Length * sizeof(T));
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SubData<T>(nint offset, ReadOnlySpan<T> data) where T : unmanaged
        {
            T* pointer = PinRange<T>(offset, (nuint)data.Length, _WRITE_ALL_ACCESS_MASK);
            data.CopyTo(new Span<T>(pointer, data.Length));
            Unpin();
        }

        #endregion


        #region Binding

        public void Bind(BufferTargetARB target) => GL.BindBuffer(target, Handle);
        public void BindRange(BufferTargetARB target, uint index, nint offset, nuint length) => GL.BindBufferRange(target, index, Handle, offset, length);
        public void BindBase(BufferTargetARB target, uint index) => GL.BindBufferBase(target, index, Handle);
        public void Unbind(BufferTargetARB target) => GL.BindBuffer(target, 0);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
