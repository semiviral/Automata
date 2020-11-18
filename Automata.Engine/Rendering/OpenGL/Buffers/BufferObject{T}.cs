using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class BufferObject<TData> : BufferObject where TData : unmanaged
    {
        public uint DataLength { get; private set; }

        public BufferObject(GL gl) : base(gl) => Handle = GL.CreateBuffer();

        public BufferObject(GL gl, uint dataLength, BufferStorageMask storageFlags = BufferStorageMask.DynamicStorageBit) : base(gl,
            dataLength * (uint)sizeof(TData), storageFlags) => DataLength = dataLength;

        public BufferObject(GL gl, Span<TData> data, BufferStorageMask storageFlags = BufferStorageMask.DynamicStorageBit) : base(gl,
            (uint)(data.Length * sizeof(TData)), storageFlags) => DataLength = (uint)data.Length;


        #region Data

        public void SetData(Span<TData> data, BufferDraw bufferDraw)
        {
            DataLength = (uint)data.Length;
            Length = DataLength * (uint)sizeof(TData);
            GL.NamedBufferData(Handle, DataLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void SubData(int offset, Span<TData> data) =>
            GL.NamedBufferSubData(Handle, offset * sizeof(TData), (uint)(data.Length * sizeof(TData)), ref data[0]);

        #endregion
    }
}
