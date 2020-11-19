using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Automata.Engine.Collections
{
    public class Palette<T> : IReadOnlyCollection<T>, IDisposable where T : IEquatable<T>
    {
        private const byte _UINT_32_BITS = sizeof(uint) * 8;

        private readonly List<T> _LookupTable;

        private byte _IndexBits;
        private uint _IndexMask;
        private uint[]? _InternalArray;

        public int Count { get; }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                uint value = GetValue(index, _IndexBits, _IndexMask, _InternalArray);
                return _LookupTable[(int)value];
            }
            set
            {
                if (index >= Count)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                int paletteIndex = _LookupTable.IndexOf(value);

                if (paletteIndex == -1)
                {
                    AllocateLookupEntry(value);
                    paletteIndex = _LookupTable.IndexOf(value);
                }

                SetValue(index, (uint)paletteIndex, _IndexBits, _IndexMask, _InternalArray);
            }
        }

        public int LookupTableSize => _LookupTable.Count;

        public Palette(int length, T defaultItem)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non negative and greater than zero.");
            }

            Count = length;
            _IndexBits = 1;
            ComputeMask();

            _LookupTable = new List<T>
            {
                defaultItem
            };

            ReallocatePalette(Compute32BitSlices(_IndexBits, Count));
        }

        public Palette(int length, IReadOnlyCollection<T> lookupTable)
        {
            if (lookupTable.Count == 0)
            {
                throw new ArgumentException("Lookup table cannot be empty.", nameof(lookupTable));
            }
            else if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non negative and greater than zero.");
            }

            Count = length;
            _IndexBits = 1;
            ComputeMask();
            _LookupTable = new List<T>(lookupTable);

            // ensure palette can fit lookup table
            while (_IndexMask < lookupTable.Count)
            {
                IncreaseIndexBits();
            }

            ReallocatePalette(Compute32BitSlices(_IndexBits, Count));
        }

        public T GetLookupIndex(int index) => _LookupTable[index];

        private void AllocateLookupEntry(T item)
        {
            Debug.Assert(!_LookupTable.Contains(item), "Lookup table already contains item. This method should only be called when the item is not present.");

            _LookupTable.Add(item);

            // check if lookup table length exceeds palette
            if (_LookupTable.Count <= _IndexMask)
            {
                return;
            }

            // expand palette
            byte oldBits = _IndexBits;
            uint oldMask = _IndexMask;

            IncreaseIndexBits();

            Span<uint> palette = stackalloc uint[Compute32BitSlices(_IndexBits, Count)];

            for (int index = 0; index < Count; index++)
            {
                uint value = GetValue(index, oldBits, oldMask, _InternalArray);
                SetValue(index, value, _IndexBits, _IndexMask, palette);
            }

            ReallocatePalette(palette.Length);
            palette.CopyTo(_InternalArray);
        }

        private void ReallocatePalette(int length)
        {
            if (_InternalArray is not null)
            {
                ArrayPool<uint>.Shared.Return(_InternalArray);
            }

            _InternalArray = ArrayPool<uint>.Shared.Rent(length);
            Array.Clear(_InternalArray, 0, _InternalArray.Length);
        }

        private static void SetValue(int index, uint lookupIndex, byte bits, uint mask, Span<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int offset = (index - (paletteIndex * (_UINT_32_BITS / bits))) * bits;
            palette[paletteIndex] = (palette[paletteIndex] & ~(mask << offset)) | (lookupIndex << offset);

            Debug.Assert(GetValue(index, bits, mask, palette).Equals(lookupIndex), $"{nameof(SetValue)} failed.");
        }

        private static uint GetValue(int index, byte bits, uint mask, ReadOnlySpan<uint> palette)
        {
            int paletteIndex = (index * bits) / _UINT_32_BITS;
            int offset = (index - (paletteIndex * (_UINT_32_BITS / bits))) * bits;

            return (uint)((palette[paletteIndex] & (mask << offset)) >> offset);
        }

        private void IncreaseIndexBits()
        {
            if (_IndexBits == 32)
            {
                throw new OverflowException($"Too many palette entries. Cannot exceed {int.MaxValue}.");
            }

            _IndexBits <<= 1;
            ComputeMask();
        }

        private void ComputeMask()
        {
            _IndexMask = 0;

            for (ushort bit = _IndexBits; bit > 0; bit >>= 1)
            {
                _IndexMask |= bit;
            }
        }

        private static ushort Compute32BitSlices(byte bits, int length) =>
            (ushort)MathF.Ceiling((bits * length) / (float)_UINT_32_BITS);

        public void CopyTo(Span<T> destination)
        {
            if (destination.Length < Count)
            {
                throw new ArgumentException("Destination span too short.");
            }

            int index = 0;

            foreach (T item in this)
            {
                destination[index] = item;
                index += 1;
            }
        }


        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Palette<T> _Palette;
            private int _Index;
            private int _Offset;
            private T? _Current;

            public T Current => _Current!;

            object? IEnumerator.Current
            {
                get
                {
                    if (((uint)_Index == 0u) || ((uint)_Index >= (uint)_Palette.Count))
                    {
                        ThrowHelper.ThrowInvalidOperationException("Enumerable has not been enumerated.");
                    }

                    return _Current;
                }
            }

            internal Enumerator(Palette<T> palette)
            {
                if (palette._InternalArray is null)
                {
                    throw new InvalidOperationException("Palette is in an invalid state (no internal data).");
                }

                _Palette = palette;
                _Index = 0;
                _Offset = 0;
                _Current = default;
            }

            public bool MoveNext()
            {
                if ((uint)_Index >= (uint)_Palette._InternalArray!.Length)
                {
                    return false;
                }

                int lookupIndex = (int)((_Palette._InternalArray![_Index] >> _Offset) & _Palette._IndexMask);
                _Current = _Palette._LookupTable[lookupIndex];
                _Offset += _Palette._IndexBits;

                if ((uint)_Offset >= _UINT_32_BITS)
                {
                    _Index += 1;
                    _Offset = 0;
                }

                return true;
            }

            void IEnumerator.Reset()
            {
                _Index = 0;
                _Current = default;
            }


            #region IDisposable

            public void Dispose() { }

            #endregion
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (_InternalArray is not null)
            {
                _LookupTable.Clear();
                ArrayPool<uint>.Shared.Return(_InternalArray);
                _InternalArray = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
