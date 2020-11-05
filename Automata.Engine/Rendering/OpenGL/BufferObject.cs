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

    public class BufferObject<TData> : IDisposable where TData : unmanaged
    {
        private readonly GL _GL;

        public uint Handle { get; }
        public uint Length { get; private set; }
        public uint ByteLength { get; private set; }

        public BufferObject(GL gl)
        {
            _GL = gl;
            Handle = _GL.CreateBuffer();
        }

        public unsafe void SetBufferData(Span<TData> data, BufferDraw bufferDraw)
        {
            Length = (uint)data.Length;
            ByteLength = Length * (uint)sizeof(TData);
            _GL.NamedBufferData(Handle, ByteLength, data, (VertexBufferObjectUsage)bufferDraw);
        }

        public void Dispose() => _GL.DeleteBuffer(Handle);
    }
}
