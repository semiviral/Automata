#region

using System;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public enum BufferDraw
    {
        /// <summary>
        ///     Draw type used when buffer data will be changed every frame (i.e. particles).
        /// </summary>
        StreamDraw = 35040,

        /// <summary>
        ///     Draw type to use when buffer data will never be updated (i.e. static scene geometry).
        /// </summary>
        StaticDraw = 35044,

        /// <summary>
        ///     Draw type to use when buffer will be updated periodically (i.e. chunks).
        /// </summary>
        DynamicDraw = 35048
    }

    public class BufferObject<TData> : OpenGLObject, IDisposable where TData : unmanaged
    {
        private bool _Disposed;

        public uint Length { get; protected set; }
        public uint ByteLength { get; protected set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public unsafe void SetBufferData(Span<TData> data, BufferDraw bufferDraw)
        {
            // todo
            // new VertexAttribute<uint>(2, 1, (uint)sizeof(DrawElementsIndirectCommand)).Commit(GL, Handle);
            // GL.VertexArrayAttribBinding(Handle, 2u, 0u);
            // GL.VertexArrayBindingDivisor(Handle, 2u, 1u);
            // GL.EnableVertexArrayAttrib(Handle, 2u);

            Length = (uint)data.Length;
            ByteLength = Length * (uint)sizeof(TData);
            GL.NamedBufferData(Handle, ByteLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public unsafe void SetBufferData(int offset, Span<TData> data) => GL.NamedBufferSubData(Handle, data.Length * sizeof(TData), ByteLength, ref data[0]);

        public unsafe void SetBufferData(Span<(int, TData)> datas)
        {
            foreach ((int offset, TData data) in datas) GL.NamedBufferSubData(Handle, offset, (uint)sizeof(TData), &data);
        }

        public unsafe void SetBufferData(uint length, uint indexSize, void* data, BufferDraw bufferDraw)
        {
            Length = length;
            ByteLength = length * indexSize;
            GL.NamedBufferData(Handle, ByteLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public unsafe void SetBufferData(int offset, uint length, uint indexSize, void* data)
        {
            Length = length;
            ByteLength = length * indexSize;
            GL.NamedBufferSubData(Handle, offset, ByteLength, data);
        }

        public void Dispose()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());

            GL.DeleteBuffer(Handle);
            _Disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
