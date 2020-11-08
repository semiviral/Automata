using System;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Collections
{
    public class Palette
    {
        private const byte _UINT_32_BITS = sizeof(uint) * 8;

        private byte _IndexBits;
        private uint _IndexCount;
        private uint _IndexMask;
        private readonly uint _Length;
        private uint[] _Palette;

        public Palette(uint length)
        {
            _IndexBits = 8;
            _IndexCount = 1u << _IndexBits;
            ComputeMask();
            _Length = length;


            _Palette = new uint[ComputeRealLength(_IndexBits, _Length)];
        }

        private void ExpandPalette()
        {
            _IndexBits *= 2;
            _IndexCount = (ushort)(1u << _IndexBits);
            ComputeMask();

            // compute mask
            for (ushort bit = _IndexBits; bit > 0; bit >>= 1) _IndexMask |= bit;

            ushort realLength = ComputeRealLength(_IndexBits, _Length);
            Span<uint> palette = stackalloc uint[realLength];
        }

        public void SetValue(int index, uint value)
        {
            if (value > _IndexCount) throw new ArgumentOutOfRangeException(nameof(value));

            int paletteIndex = (index * _IndexBits) / _UINT_32_BITS;
            int offset = (index - paletteIndex) * _IndexBits;
            uint offsetUnaryMask = ~(_IndexMask << offset);
            _Palette[paletteIndex] = (_Palette[paletteIndex] & offsetUnaryMask) | (value << offset);
        }

        public uint GetValue(int index)
        {
            int paletteIndex = (index * _IndexBits) / _UINT_32_BITS;
            int remainder = index - paletteIndex;
            int offset = remainder * _IndexBits;

            return (uint)((_Palette[paletteIndex] & (_IndexMask << offset)) >> offset);
        }

        private void ComputeMask()
        {
            _IndexMask = 0;
            for (ushort bit = _IndexBits; bit > 0; bit >>= 1) _IndexMask |= bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ComputeRealLength(byte bits, uint length) =>
            (ushort)MathF.Ceiling((bits * length) / (float)_UINT_32_BITS);
    }
}
