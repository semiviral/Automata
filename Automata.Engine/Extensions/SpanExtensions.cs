using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Automata.Engine.Extensions
{
    public static class SpanExtensions
    {
        private static readonly int _VectorRegisterSizeX1 = Vector<byte>.Count * 1;
        private static readonly int _VectorRegisterSizeX2 = Vector<byte>.Count * 2;
        private static readonly int _VectorRegisterSizeX3 = Vector<byte>.Count * 3;
        private static readonly int _VectorRegisterSizeX4 = Vector<byte>.Count * 4;

        public static void VectorCopy<T>(this Span<T> source, Span<T> destination) where T : unmanaged =>
            VectorCopy(MemoryMarshal.AsBytes(source), MemoryMarshal.AsBytes(destination));

        public static void VectorCopy(this Span<byte> source, Span<byte> destination)
        {
            if (source.Length > destination.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination), "Destination too short.");
            }

            int count = source.Length;
            int offset = 0;

            for (; count >= _VectorRegisterSizeX4; count -= _VectorRegisterSizeX4, offset += _VectorRegisterSizeX4)
            {
                new Vector<byte>(source.Slice(offset)).CopyTo(destination.Slice(offset));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX1)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX1));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX2)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX2));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX3)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX3));
            }

            if (count is 0)
            {
                return;
            }

            if (count >= _VectorRegisterSizeX3)
            {
                new Vector<byte>(source.Slice(offset)).CopyTo(destination.Slice(count));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX1)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX1));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX2)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX2));
                count -= _VectorRegisterSizeX3;
                offset += _VectorRegisterSizeX3;
            }

            if (count >= _VectorRegisterSizeX2)
            {
                new Vector<byte>(source.Slice(offset)).CopyTo(destination.Slice(count));
                new Vector<byte>(source.Slice(offset + _VectorRegisterSizeX1)).CopyTo(destination.Slice(offset + _VectorRegisterSizeX1));
                count -= _VectorRegisterSizeX2;
                offset += _VectorRegisterSizeX2;
            }

            if (count >= _VectorRegisterSizeX1)
            {
                new Vector<byte>(source.Slice(offset)).CopyTo(destination.Slice(count));
                count -= _VectorRegisterSizeX1;
                offset += _VectorRegisterSizeX1;
            }

            for (; count > 0; count -= 1, offset += 1)
            {
                destination[offset] = source[offset];
            }
        }
    }
}
