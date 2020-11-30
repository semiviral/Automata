using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable ConvertToAutoProperty

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vector2<T> : IEquatable<Vector2<T>>, IFormattable where T : unmanaged
    {
        public static Vector2<T> Zero => new Vector2<T>(default);
        public static Vector2<T> One => new Vector2<T>(Primitive<T>.One);
        public static Vector2<T> UnitX => new Vector2<T>(Primitive<T>.One, default);
        public static Vector2<T> UnitY => new Vector2<T>(default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;

        public T X => _X;
        public T Y => _Y;

        public Vector2(T xy)
        {
            _X = xy;
            _Y = xy;
        }

        public Vector2(T x, T y)
        {
            _X = x;
            _Y = y;
        }

        public Vector2<T> WithX(T x) => new Vector2<T>(x, _Y);
        public Vector2<T> WithY(T y) => new Vector2<T>(_X, y);

        public override int GetHashCode() => HashCode.Combine(_X, _Y);
        public override bool Equals(object? obj) => obj is Vector2<T> other && Equals(other);
        public override string ToString() => $"<{X}, {Y}>";


        #region IEquatable

        public bool Equals(Vector2<T> other) => this == other;

        #endregion


        #region IFormattable

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            builder.Append('<');
            builder.Append((_X as IFormattable)!.ToString(format, formatProvider));
            builder.Append(separator);
            builder.Append(' ');
            builder.Append((_Y as IFormattable)!.ToString(format, formatProvider));
            builder.Append('>');
            return builder.ToString();
        }

        #endregion


        #region Operators

        #region Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2<T> a, T b) => a == new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(T a, Vector2<T> b) => new Vector2<T>(a) == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2<T> a, Vector2<T> b) => Vector.EqualsInternal(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2<T> a, T b) => a != new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(T a, Vector2<T> b) => new Vector2<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2<T> a, Vector2<T> b) => Vector.NotEqualsInternal(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(Vector2<T> a, T b) => a + new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(T a, Vector2<T> b) => new Vector2<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(Vector2<T> a, Vector2<T> b) => Vector.AddInternal(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(Vector2<T> a, T b) => a - new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(T a, Vector2<T> b) => new Vector2<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(Vector2<T> a, Vector2<T> b) => Vector.SubtractInternal(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(Vector2<T> a, T b) => a * new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(T a, Vector2<T> b) => new Vector2<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(Vector2<T> a, Vector2<T> b) => Vector.MultiplyInternal(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(Vector2<T> a, T b) => a / new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(T a, Vector2<T> b) => new Vector2<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(Vector2<T> a, Vector2<T> b) => Vector.DivideInternal(a, b);

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(Vector2<T> a, T b) => a > new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(T a, Vector2<T> b) => new Vector2<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(Vector2<T> a, Vector2<T> b) => Vector.GreaterThanInternal(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(Vector2<T> a, T b) => a < new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(T a, Vector2<T> b) => new Vector2<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(Vector2<T> a, Vector2<T> b) => Vector.LessThanInternal(a, b);
        #endregion

        #endregion
    }
}
