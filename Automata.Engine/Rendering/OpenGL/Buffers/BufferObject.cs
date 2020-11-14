using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class BufferObject<TData> : OpenGLObject, IDisposable where TData : unmanaged
    {
        private bool _Disposed;

        public uint Length { get; private set; }
        public uint ByteLength { get; private set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public BufferObject(GL gl, uint length, BufferStorageMask bufferStorageMask = BufferStorageMask.DynamicStorageBit)
            : this(gl, length, Span<TData>.Empty, bufferStorageMask) { }

        public BufferObject(GL gl, uint length, Span<TData> data, BufferStorageMask bufferStorageMask = BufferStorageMask.DynamicStorageBit) : base(gl)
        {
            Length = length;
            ByteLength = length * (uint)sizeof(TData);

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, ByteLength, data, (uint)bufferStorageMask);
        }

        public void SetBufferData(Span<TData> data, BufferDraw bufferDraw)
        {
            Length = (uint)data.Length;
            ByteLength = Length * (uint)sizeof(TData);
            GL.NamedBufferData(Handle, ByteLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SetBufferData(int offset, Span<TData> data) =>
            GL.NamedBufferSubData(Handle, offset * sizeof(TData), (uint)(data.Length * sizeof(TData)), ref data[0]);

        public void SetBufferData(Span<(int, TData)> data)
        {
            foreach ((int offset, TData datum) in data) GL.NamedBufferSubData(Handle, offset, (uint)sizeof(TData), &datum);
        }

        public void SetBufferData(uint length, uint indexSize, void* data, BufferDraw bufferDraw)
        {
            Length = length;
            ByteLength = length * indexSize;
            GL.NamedBufferData(Handle, ByteLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SetBufferData(int offset, uint length, uint indexSize, void* data)
        {
            Length = length;
            ByteLength = length * indexSize;
            GL.NamedBufferSubData(Handle, offset, ByteLength, data);
        }

        public void Bind(BufferTargetARB target) => GL.BindBuffer(target, Handle);
        public void Unbind(BufferTargetARB target) => GL.BindBuffer(target, 0);

        public void Dispose()
        {
            if (_Disposed) return;

            GC.SuppressFinalize(this);
            GL.DeleteBuffer(Handle);
            _Disposed = true;
        }
    }
}
