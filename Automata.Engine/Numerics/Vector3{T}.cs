using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Automata.Engine.Numerics
{
    public readonly struct Vector3<T> : IEquatable<Vector3<T>>, IFormattable where T : unmanaged
    {
        public static Vector3<T> Zero => new Vector3<T>(default);
        public static Vector3<T> One => new Vector3<T>(Primitive<T>.One);
        public static Vector3<T> UnitX => new Vector3<T>(Primitive<T>.One, default, default);
        public static Vector3<T> UnitY => new Vector3<T>(default, Primitive<T>.One, default);
        public static Vector3<T> UnitZ => new Vector3<T>(default, default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;
        private readonly T _Z;

        public T X => _X;
        public T Y => _Y;
        public T Z => _Z;

        public Vector3(T xyz)
        {
            _X = xyz;
            _Y = xyz;
            _Z = xyz;
        }

        public Vector3(T x, T y, T z)
        {
            _X = x;
            _Y = y;
            _Z = z;
        }

        public Vector3<T> WithX(T x) => new Vector3<T>(x, _Y, _Z);
        public Vector3<T> WithY(T y) => new Vector3<T>(_X, y, _Z);
        public Vector3<T> WithZ(T z) => new Vector3<T>(_X, _Y, z);

        public override int GetHashCode() => HashCode.Combine(_X, _Y);
        public override bool Equals(object? obj) => obj is Vector3<T> other && Equals(other);
        public override string ToString() => $"<{X}, {Y}, {Z}>";


        #region IEquatable

        public bool Equals(Vector3<T> other) => this == other;

        #endregion


        #region IFormattable

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            builder.Append('<');
            builder.Append((X as IFormattable)!.ToString(format, formatProvider));
            builder.Append(separator);
            builder.Append(' ');
            builder.Append((Y as IFormattable)!.ToString(format, formatProvider));
            builder.Append(separator);
            builder.Append(' ');
            builder.Append((Z as IFormattable)!.ToString(format, formatProvider));
            builder.Append('>');
            return builder.ToString();
        }

        #endregion


        #region Operators

        #region Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3<T> a, T b) => a == new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(T a, Vector3<T> b) => new Vector3<T>(a) == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3<T> a, Vector3<T> b) => Vector.EqualsInternal(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3<T> a, T b) => a != new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(T a, Vector3<T> b) => new Vector3<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3<T> a, Vector3<T> b) => Vector.NotEqualsInternal(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(Vector3<T> a, T b) => a + new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(T a, Vector3<T> b) => new Vector3<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(Vector3<T> a, Vector3<T> b) => Vector.AddInternal(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(Vector3<T> a, T b) => a - new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(T a, Vector3<T> b) => new Vector3<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(Vector3<T> a, Vector3<T> b) => Vector.SubtractInternal(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(Vector3<T> a, T b) => a * new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(T a, Vector3<T> b) => new Vector3<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(Vector3<T> a, Vector3<T> b) => Vector.MultiplyInternal(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(Vector3<T> a, T b) => a / new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(T a, Vector3<T> b) => new Vector3<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(Vector3<T> a, Vector3<T> b) => Vector.DivideInternal(a, b);

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(Vector3<T> a, T b) => a > new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(T a, Vector3<T> b) => new Vector3<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(Vector3<T> a, Vector3<T> b) => Vector.GreaterThanInternal(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(Vector3<T> a, T b) => a < new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(T a, Vector3<T> b) => new Vector3<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(Vector3<T> a, Vector3<T> b) => Vector.LessThanInternal(a, b);

        #endregion

        #endregion
    }
}
