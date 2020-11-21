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

        public nuint Length { get; }

        public bool IsEmpty => Length is 0u;

        public ref T this[nuint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index >= Length)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                return ref *(_Pointer + index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(T* pointer, nuint length)
        {
            _Pointer = pointer;
            Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(ref T pointer, nuint length)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref pointer);
            Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(Span<T> span)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            Length = (nuint)span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan(T[] array)
        {
            _Pointer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array));
            Length = (nuint)array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(NativeSpan<T> destination)
        {
            if (destination._Pointer == _Pointer)
            {
                // there's no point copying inside itself
                return;
            }

            if (destination.Length > Length)
            {
                ThrowHelper.ThrowDestinationTooShort();
            }
            else
            {
                nuint length = destination.Length * (nuint)sizeof(T);
                Buffer.MemoryCopy(_Pointer, destination._Pointer, length, length);
            }
        }

        public void Clear(T value = default)
        {
            nuint index = Length - 1u;

            // decrement downwards, down to >= our decrement alignment
            // this allows us to avoid underflow
            for (; index >= 7u; index -= 8u)
            {
                *(_Pointer + (index - 0u)) = value;
                *(_Pointer + (index - 1u)) = value;
                *(_Pointer + (index - 2u)) = value;
                *(_Pointer + (index - 3u)) = value;
                *(_Pointer + (index - 4u)) = value;
                *(_Pointer + (index - 5u)) = value;
                *(_Pointer + (index - 6u)) = value;
                *(_Pointer + (index - 7u)) = value;
            }

            for (; index is < 7u and >= 0u; index--)
            {
                *(_Pointer + index) = value;
            }

            if (index + 1u is not 0u)
            {
                ThrowHelper.ThrowInvalidOperationException("Clear failed unexpectedly.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            if (Length is 0u)
            {
                return Span<T>.Empty;
            }
            else if (Length > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Length),
                    $"{nameof(NativeSpan<T>)} is too large to index as a span.");
            }

            return new Span<T>(_Pointer, (int)Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (Length is 0u)
            {
                return Array.Empty<T>();
            }
            else if (Length > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(Length),
                    $"{nameof(NativeSpan<T>)} is too large to index as an array.");
            }

            T[] destination = new T[(int)Length];
            CopyTo(new NativeSpan<T>(destination));
            return destination;
        }

        public override string ToString() => $"{nameof(NativeSpan<T>)}({string.Join(", ", ToArray())})";


        #region Slice

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan<T> Slice(nuint start)
        {
            if (start >= Length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            return new NativeSpan<T>(_Pointer + start, Length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpan<T> Slice(nuint start, nuint length)
        {
            if ((start + length) >= Length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            return new NativeSpan<T>(_Pointer + start, length);
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


        #region Conversions

        public static implicit operator NativeSpan<T>(T[] array) => new NativeSpan<T>(array);
        public static implicit operator NativeSpan<T>(Span<T> span) => new NativeSpan<T>(span);

        #endregion
    }
}
