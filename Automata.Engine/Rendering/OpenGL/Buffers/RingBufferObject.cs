using System;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public enum BufferingScheme : uint
    {
        Double = 2u,
        Triple = 3u,
        Quadruple = 4u
    }

    public class Ring
    {
        private readonly nuint _Max;

        public nuint Current { get; private set; }

        public Ring(nuint max) => _Max = max;

        public void Increment() => Current = (Current + 1u) % _Max;
    }

    public class RingBufferObject : OpenGLObject
    {
        private const MapBufferAccessMask _ACCESS_MASK = MapBufferAccessMask.MapWriteBit
                                                         | MapBufferAccessMask.MapPersistentBit
                                                         | MapBufferAccessMask.MapCoherentBit;

        private Ring CurrentSlice { get; }

        public nuint Size { get; }
        public nuint BufferedSize { get; }
        public BufferingScheme BufferingScheme { get; }

        public RingBufferObject(GL gl, nuint size, BufferingScheme bufferingScheme) : base(gl)
        {
            BufferingScheme = bufferingScheme;
            Size = size;
            BufferedSize = size * (uint)bufferingScheme;
            CurrentSlice = new Ring((nuint)bufferingScheme);

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, BufferedSize, Span<byte>.Empty, (uint)_ACCESS_MASK);
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            if ((nuint)data.Length != Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segments into ring buffer; your data may be too small or large.");
            }
        }
    }
}
