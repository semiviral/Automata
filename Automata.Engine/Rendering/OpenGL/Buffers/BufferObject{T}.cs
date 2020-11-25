using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class BufferObject<TData> : BufferObject where TData : unmanaged
    {
        public nuint DataLength { get; private set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public BufferObject(GL gl, ReadOnlySpan<TData> data, BufferStorageMask flags) : base(gl)
        {
            DataLength = (nuint)data.Length;
            Length = DataLength * (nuint)sizeof(TData);
            GL.NamedBufferStorage(Handle, Length, data, (uint)flags);
        }

        public BufferObject(GL gl, ReadOnlySpan<TData> data, BufferDraw usage) : this(gl)
        {
            DataLength = (nuint)data.Length;
            Length = DataLength * (nuint)sizeof(TData);
            GL.NamedBufferData(Handle, Length, data, (VertexBufferObjectUsage)usage);
        }

        public BufferObject(GL gl, nuint dataLength, BufferStorageMask flags) : base(gl)
        {
            DataLength = dataLength;
            Length = DataLength * (nuint)sizeof(TData);
            GL.NamedBufferStorage(Handle, Length, (void*)null!, (uint)flags);
        }

        public BufferObject(GL gl, nuint dataLength, BufferDraw usage) : this(gl)
        {
            DataLength = dataLength;
            Length = DataLength * (nuint)sizeof(TData);
            GL.NamedBufferData(Handle, Length, (void*)null!, (VertexBufferObjectUsage)usage);
        }

        #region Data

        public void SetData(ReadOnlySpan<TData> data, BufferDraw bufferDraw)
        {
            DataLength = (nuint)data.Length;
            base.SetData(data, bufferDraw);
        }

        public void SubData(nint offset, ReadOnlySpan<TData> data) => base.SubData(offset * sizeof(TData), data);

        #endregion
    }
}
