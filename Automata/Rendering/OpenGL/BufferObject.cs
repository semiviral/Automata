#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
    {
        private readonly BufferTargetARB _BufferType;
        private readonly GL _GL;
        private readonly uint _Handle;

        public uint Length { get; private set; }

        public BufferObject(GL gl, BufferTargetARB bufferType)
        {
            _GL = gl;
            _BufferType = bufferType;
            _Handle = _GL.GenBuffer();
            Bind();
        }

        public void Dispose()
        {
            _GL.DeleteBuffer(_Handle);
        }

        public unsafe void SetBufferData(Span<TDataType> data)
        {
            fixed (void* dataPtr = data)
            {
                Length = (uint)(data.Length * sizeof(TDataType));
                _GL.BufferData(_BufferType, Length, dataPtr, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            _GL.BindBuffer(_BufferType, _Handle);
        }
    }
}
