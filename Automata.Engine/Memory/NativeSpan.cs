using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Automata.Engine.Memory
{
    public static class SpanExtensions
    {
        public static NativeSpan<T> AsNative<T>(this Span<T> span) where T : unmanaged =>
            new NativeSpan<T>(ref span.GetPinnableReference(), (nuint)span.Length);
    }

    public readonly unsafe ref struct NativeSpan<T> where T : unmanaged
    {
        public static NativeSpan<T> Empty => default;

        private readonly T* _Pointer;
        private readonly nuint _Length;

        public nuint Length => _Length;
        public bool IsEmpty => _Length is 0u;

        public ref T this[nuint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index >= _Length)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                return ref Unsafe.AsRef<T>(NativeUnsafe.Add(_Pointer, index));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(T* pointer, nuint length)
        {
            _Pointer = pointer;
            _Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(ref T pointer, nuint length)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref pointer);
            _Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(Span<T> span)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            _Length = (nuint)span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(T[] array)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array));
            _Length = (nuint)array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(NativeSpan<T> destination)
        {
            if (destination.Length > _Length)
            {
                ThrowHelper.ThrowDestinationTooShort();
            }
            else
            {
                Buffer.MemoryCopy(_Pointer, destination._Pointer, destination.Length, destination.Length);
            }
        }

        public void CopyTo(Span<T> destination) => CopyTo(destination.AsNative());

        public void Clear(T value = default)
        {
            nuint index = _Length - 1u;

            // decrement downwards, down to >= our decrement alignment
            // this allows us to avoid underflow
            for (; index >= 7u; index -= 8u)
            {
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 0u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 1u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 2u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 3u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 4u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 5u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 6u), value);
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index + 7u), value);
            }

            for (; index >= 0; index--)
            {
                Unsafe.Write(NativeUnsafe.Add(_Pointer, index), value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            if (_Length is 0u)
            {
                return Span<T>.Empty;
            }
            else if (_Length > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Length),
                    $"{nameof(NativeSpan<T>)} is too large to index as a span.");
            }

            return new Span<T>(_Pointer, (int)_Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (_Length is 0u)
            {
                return Array.Empty<T>();
            }
            else if (_Length > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Length),
                    $"{nameof(NativeSpan<T>)} is too large to index as an array.");
            }

            T[] destination = new T[(int)_Length];
            CopyTo(new NativeSpan<T>(destination));
            return destination;
        }


        #region Slice

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan<T> Slice(nuint start)
        {
            if (start >= Length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            return new NativeSpan<T>(NativeUnsafe.Add(_Pointer, start), _Length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan<T> Slice(nuint start, nuint length)
        {
            if ((start + length) >= Length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            return new NativeSpan<T>(NativeUnsafe.Add(_Pointer, start), length);
        }

        #endregion


        #region IEnumerable

        public Enumerator GetEnumerator() => new Enumerator(this);

        public ref struct Enumerator
        {
            private readonly NativeSpan<T> _Span;
            private nuint _Index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="span">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeSpan<T> span)
            {
                _Span = span;
                _Index = (nuint)0u;
            }

            /// <summary>Advances the enumerator to the next element of the span.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_Index == (_Span.Length - 1))
                {
                    return false;
                }

                _Index += 1u;
                return true;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _Span[_Index];
            }
        }

        #endregion


        #region NotSupported

        public override int GetHashCode() => throw new NotSupportedException();

        #endregion
    }
}
