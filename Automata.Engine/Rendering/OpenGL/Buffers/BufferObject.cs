using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class BufferObject : OpenGLObject
    {
        public uint Length { get; protected set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public BufferObject(GL gl, uint length, BufferStorageMask storageFlags = BufferStorageMask.DynamicStorageBit) : base(gl)
        {
            Length = length;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, Length, (void*)null!, (uint)storageFlags);
        }

        public BufferObject(GL gl, void* data, uint length, BufferStorageMask storageFlags = BufferStorageMask.DynamicStorageBit) : base(gl)
        {
            Length = length;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, Length, data, (uint)storageFlags);
        }

        public void Resize(uint length, BufferDraw bufferDraw)
        {
            Length = length;
            GL.NamedBufferData(Handle, Length, (void*)null!, (VertexBufferObjectUsage)bufferDraw);
        }

        public void* Pin(BufferAccessARB access) => GL.MapNamedBuffer(Handle, access);
        public Span<T> Pin<T>(BufferAccessARB access) where T : unmanaged => new Span<T>(GL.MapNamedBuffer(Handle, access), (int)Length);
        public void Unpin() => GL.UnmapNamedBuffer(Handle);


        #region Data

        public void SetData(void* data, uint length, BufferDraw bufferDraw)
        {
            Length = length;
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SubData(int offset, void* data, uint length) => GL.NamedBufferSubData(Handle, offset, length, data);

        public void SetData<T>(Span<T> data, BufferDraw bufferDraw) where T : unmanaged
        {
            Length = (uint)(data.Length * sizeof(T));
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SubData<T>(int offset, Span<T> data) where T : unmanaged =>
            GL.NamedBufferSubData(Handle, offset, (uint)(data.Length * sizeof(T)), ref data.GetPinnableReference());

        #endregion


        #region Binding

        public void Bind(BufferTargetARB target) => GL.BindBuffer(target, Handle);
        public void Unbind(BufferTargetARB target) => GL.BindBuffer(target, 0);

        #endregion


        #region IDisposable

        protected override void SafeDispose() => GL.DeleteBuffer(Handle);

        #endregion
    }
}
