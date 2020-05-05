#region

using System;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering.OpenGL
{
    public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
    {
        private readonly uint _Handle;
        private readonly BufferTargetARB _BufferType;
        private readonly GL _GL;

        public BufferObject(GL gl, BufferTargetARB bufferType)
        {
            _GL = gl;
            _BufferType = bufferType;

            _Handle = _GL.GenBuffer();
            Bind();
        }

        public unsafe void SetBufferData(Span<TDataType> data)
        {
            fixed (void* d = data)
            {
                _GL.BufferData(_BufferType, (uint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            _GL.BindBuffer(_BufferType, _Handle);
        }

        public void Dispose()
        {
            _GL.DeleteBuffer(_Handle);
        }
    }
}
