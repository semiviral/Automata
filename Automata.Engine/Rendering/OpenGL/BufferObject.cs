#region

using System;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering.OpenGL
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

        public unsafe void SetBufferData(Span<TDataType> data, BufferDraw bufferDraw)
        {
            Length = (uint)(data.Length * sizeof(TDataType));
            _GL.BufferData(_BufferType, Length, data, (BufferUsageARB)bufferDraw);
        }

        public void Bind() => _GL.BindBuffer(_BufferType, _Handle);
        public void Dispose() => _GL.DeleteBuffer(_Handle);
    }
}
