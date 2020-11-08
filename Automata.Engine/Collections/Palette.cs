using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Collections
{
    public class Palette<T> : IReadOnlyCollection<T> where T : IEquatable<T>
    {
        private const byte _UINT_32_BITS = sizeof(uint) * 8;

        private readonly uint _Length;
        private readonly List<T> _LookupTable;

        private byte _IndexBits;
        private uint _IndexMask;
        private uint[] _Palette;

        public int Count => (int)_Length;

        public Palette(uint length, T defaultItem)
        {
            _Length = length;
            _IndexBits = 1;
            ComputeMask();
            _Palette = new uint[ComputeRealLength(_IndexBits, _Length)];

            _LookupTable = new List<T>
            {
                defaultItem
            };
        }

        public Palette(uint length, IReadOnlyCollection<T> lookupTable)
        {
            if (lookupTable.Count == 0) throw new ArgumentException("Lookup table cannot be empty.", nameof(lookupTable));

            _Length = length;
            _IndexBits = 1;
            ComputeMask();

            // ensure palette can fit lookup table
            while (_IndexMask < lookupTable.Count) IncreaseIndexBits();

            _Palette = new uint[ComputeRealLength(_IndexBits, _Length)];
            _LookupTable = new List<T>(lookupTable);
        }

        public void SetValue(int index, T item)
        {
            if (index >= _Length) throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the collection.");

            int paletteIndex = _LookupTable.IndexOf(item);

            if (paletteIndex == -1)
            {
                AllocateLookupEntry(item);
                paletteIndex = _LookupTable.IndexOf(item);
            }

            SetValue(index, (uint)paletteIndex, _IndexBits, _IndexMask, _Palette);
        }

        public T GetValue(int index)
        {
            if (index >= _Length) throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the collection.");

            uint value = GetValue(index, _IndexBits, _IndexMask, _Palette);
            return _LookupTable[(int)value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateLookupEntry(T entry)
        {
#if DEBUG
            if (_LookupTable.Contains(entry)) throw new ArgumentException("Lookup table already contains entry.", nameof(entry));
#endif

            _LookupTable.Add(entry);

            // check if lookup table length exceeds palette
            if (_LookupTable.Count <= _IndexMask) return;

            // expand palette
            byte oldBits = _IndexBits;
            uint oldMask = _IndexMask;

            IncreaseIndexBits();

            Span<uint> palette = stackalloc uint[ComputeRealLength(_IndexBits, _Length)];

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
            int offset = (index - (paletteIndex * (_UINT_32_BITS / bits))) * bits;
            palette[paletteIndex] = (palette[paletteIndex] & ~(mask << offset)) | (value << offset);

            Debug.Assert(GetValue(index, bits, mask, palette).Equals(value), $"{nameof(SetValue)} failed.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetValue(int index, byte bits, uint mask, ReadOnlySpan<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int offset = (index - (paletteIndex * (_UINT_32_BITS / bits))) * bits;

            return (uint)((palette[paletteIndex] & (mask << offset)) >> offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseIndexBits()
        {
            _IndexBits <<= 1;
            ComputeMask();
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            foreach (uint value in _Palette)
                for (int offset = 0; offset < _UINT_32_BITS; offset += _IndexBits)
                    yield return _LookupTable[(int)((value >> offset) & _IndexMask)];
        }
    }
}
