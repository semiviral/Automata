using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Collections
{
    public class Palette<T> where T : IEquatable<T>
    {
        private const byte _UINT_32_BITS = sizeof(uint) * 8;

        private readonly uint _Length;
        private readonly List<T> _LookupTable;

        private byte _IndexBits;
        private uint _IndexMask;
        private uint[] _Palette;

        public Palette(uint length)
        {
            _Length = length;
            _IndexBits = 1;
            ComputeMask();
            _Palette = new uint[ComputeRealLength(_IndexBits, _Length)];
            _LookupTable = new List<T>();
        }

        public Palette(uint length, IReadOnlyCollection<T> lookupTable)
        {
            _Length = length;
            _IndexBits = 1;
            ComputeMask();

            // ensure palette can fit lookup table
            while (_IndexMask < lookupTable.Count)
            {
                _IndexBits *= 2;
                ComputeMask();
            }

            _Palette = new uint[ComputeRealLength(_IndexBits, _Length)];
            _LookupTable = new List<T>(lookupTable);
        }

        public void SetValue(int index, T item)
        {
            if (!_LookupTable.Contains(item)) AllocateLookupEntry(item);

            SetValue(index, (uint)_LookupTable.IndexOf(item), _IndexBits, _IndexMask, _Palette);
        }

        public T GetValue(int index)
        {
            uint value = GetValue(index, _IndexBits, _IndexMask, _Palette);
            return _LookupTable[(int)value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateLookupEntry(T entry)
        {
            if (_LookupTable.Contains(entry)) throw new ArgumentException("Lookup table already contains entry.", nameof(entry));

            _LookupTable.Add(entry);

            // check if lookup table length exceeds palette
            if (_LookupTable.Count <= _IndexMask) return;

            // expand palette
            byte oldBits = _IndexBits;
            uint oldMask = _IndexMask;

            _IndexBits *= 2;
            ComputeMask();

            // compute mask
            for (ushort bit = _IndexBits; bit > 0; bit >>= 1) _IndexMask |= bit;

            ushort realLength = ComputeRealLength(_IndexBits, _Length);
            Span<uint> palette = stackalloc uint[realLength];

            for (int index = 0; index < _Length; index++)
            {
                uint value = GetValue(index, oldBits, oldMask, _Palette);
                SetValue(index, value, _IndexBits, _IndexMask, palette);
            }

            _Palette = palette.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetValue(int index, uint value, byte bits, uint mask, Span<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int offset = (index - paletteIndex) * bits;
            uint offsetUnaryMask = ~(mask << offset);
            palette[paletteIndex] = (palette[paletteIndex] & offsetUnaryMask) | (value << offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetValue(int index, byte bits, uint mask, ReadOnlySpan<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int remainder = index - paletteIndex;
            int offset = remainder * bits;

            return (uint)((palette[paletteIndex] & (mask << offset)) >> offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
