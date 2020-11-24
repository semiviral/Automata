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

        public BufferObject(GL gl, void* data, nuint length, BufferStorageMask storageFlags = BufferStorageMask.MapWriteBit) : base(gl)
        {
            Length = length;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, Length, data, (uint)storageFlags);
        }

        public BufferObject(GL gl, nuint length, BufferStorageMask storageFlags = BufferStorageMask.MapWriteBit) :
            this(gl, (void*)null!, length, storageFlags) { }

        public void Resize(nuint length, BufferDraw bufferDraw)
        {
            Length = length;
            GL.NamedBufferData(Handle, Length, (void*)null!, (VertexBufferObjectUsage)bufferDraw);
        }


        #region Pinning

        public void* Pin(MapBufferAccessMask mask) => GL.MapNamedBufferRange(Handle, (nint)0, Length, (uint)mask);
        public T* Pin<T>(MapBufferAccessMask mask) where T : unmanaged => (T*)Pin(mask);
        public void* PinRange(nint offset, nuint length, MapBufferAccessMask mask) => GL.MapNamedBufferRange(Handle, offset, length, (uint)mask);
        public T* PinRange<T>(nint offset, nuint length, MapBufferAccessMask mask) where T : unmanaged => (T*)PinRange(offset, length * (nuint)sizeof(T), mask);
        public void Unpin() => GL.UnmapNamedBuffer(Handle);

        #endregion


        #region Data

        public void SetData(void* data, nuint length, BufferDraw bufferDraw)
        {
            Resize(length, bufferDraw);
            void* pointer = Pin(_WRITE_ALL_ACCESS_MASK);
            Buffer.MemoryCopy(data, pointer, Length, Length);
            Unpin();
        }

        public void SubData(nint offset, nuint length, void* data)
        {
            void* pointer = PinRange(offset, length, _WRITE_ALL_ACCESS_MASK);
            Buffer.MemoryCopy(data, pointer, length, length);
            Unpin();
        }

        public void SetData<T>(ReadOnlySpan<T> data, BufferDraw bufferDraw) where T : unmanaged
        {
            Resize((nuint)(data.Length * sizeof(T)), bufferDraw);
            T* pointer = Pin<T>(_WRITE_ALL_ACCESS_MASK);
            data.CopyTo(new Span<T>(pointer, data.Length));
            Unpin();
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
        public void Unbind(BufferTargetARB target) => GL.BindBuffer(target, 0);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
