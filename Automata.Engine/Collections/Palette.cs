using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Collections
{
    public class Palette<T> : IReadOnlyCollection<T> where T : IEquatable<T>
    {
        private const byte _UINT_32_BITS = sizeof(uint) * 8;

        private readonly List<T> _LookupTable;

        private byte _IndexBits;
        private uint _IndexMask;
        private uint[] _Palette;

        public int Count { get; }

        public IReadOnlyList<T> LookupTable => _LookupTable;

        public T this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the collection.");

                uint value = GetValue(index, _IndexBits, _IndexMask, _Palette);
                return _LookupTable[(int)value];
            }
            set
            {
                if (index >= Count) throw new IndexOutOfRangeException("Index must be non-negative and less than the size of the collection.");

                int paletteIndex = _LookupTable.IndexOf(value);

                if (paletteIndex == -1)
                {
                    AllocateLookupEntry(value);
                    paletteIndex = _LookupTable.IndexOf(value);
                }

                SetValue(index, (uint)paletteIndex, _IndexBits, _IndexMask, _Palette);
            }
        }

        public Palette(int length, T defaultItem)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Length must be non negative and greater than zero.");

            Count = length;
            _IndexBits = 1;
            ComputeMask();

            _LookupTable = new List<T>
            {
                defaultItem
            };

            _Palette = ArrayPool<uint>.Shared.Rent(Compute32BitSlices(_IndexBits, Count));
        }

        public Palette(int length, IReadOnlyCollection<T> lookupTable)
        {
            if (lookupTable.Count == 0) throw new ArgumentException("Lookup table cannot be empty.", nameof(lookupTable));
            else if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Length must be non negative and greater than zero.");

            Count = length;
            _IndexBits = 1;
            ComputeMask();
            _LookupTable = new List<T>(lookupTable);

            // ensure palette can fit lookup table
            while (_IndexMask < lookupTable.Count) IncreaseIndexBits();

            _Palette = ArrayPool<uint>.Shared.Rent(Compute32BitSlices(_IndexBits, Count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateLookupEntry(T item)
        {
            Debug.Assert(!_LookupTable.Contains(item), "Lookup table already contains item. This method should only be called when the item is not present.");

            _LookupTable.Add(item);

            // check if lookup table length exceeds palette
            if (_LookupTable.Count <= _IndexMask) return;

            // expand palette
            byte oldBits = _IndexBits;
            uint oldMask = _IndexMask;

            IncreaseIndexBits();

            Span<uint> palette = stackalloc uint[Compute32BitSlices(_IndexBits, Count)];

            for (int index = 0; index < Count; index++)
            {
                uint value = GetValue(index, oldBits, oldMask, _Palette);
                SetValue(index, value, _IndexBits, _IndexMask, palette);
            }

            ArrayPool<uint>.Shared.Return(_Palette, true);
            _Palette = ArrayPool<uint>.Shared.Rent(palette.Length);
            palette.CopyTo(_Palette);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetValue(int index, uint lookupIndex, byte bits, uint mask, Span<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int offset = (index - (paletteIndex * (_UINT_32_BITS / bits))) * bits;
            palette[paletteIndex] = (palette[paletteIndex] & ~(mask << offset)) | (lookupIndex << offset);

            Debug.Assert(GetValue(index, bits, mask, palette).Equals(lookupIndex), $"{nameof(SetValue)} failed.");
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
            if (_IndexBits == 32) throw new OverflowException($"Too many palette entries. Cannot exceed {int.MaxValue}.");

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
        private static ushort Compute32BitSlices(byte bits, int length) =>
            (ushort)MathF.Ceiling((bits * length) / (float)_UINT_32_BITS);

        public void Clear(T defaultItem)
        {
            _IndexBits = 1;
            ComputeMask();

            _LookupTable.Clear();
            _LookupTable.Add(defaultItem);
            _Palette = ArrayPool<uint>.Shared.Rent(Compute32BitSlices(_IndexBits, Count));
        }

        public void CopyTo(Span<T> destination)
        {
            if (destination.Length < Count) throw new ArgumentException("Destination span too short.");

            int index = 0;

            foreach (T item in this)
            {
                destination[index] = item;
                index += 1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            int index = 0;

            foreach (uint value in _Palette)
            {
                for (int offset = 0; offset < _UINT_32_BITS; offset += _IndexBits)
                {
                    yield return _LookupTable[(int)((value >> offset) & _IndexMask)];

                    if ((index += 1) >= Count) yield break;
                }
            }
        }

        ~Palette() => ArrayPool<uint>.Shared.Return(_Palette, true);
    }
}
