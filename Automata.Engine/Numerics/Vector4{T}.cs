using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector4<T> where T : unmanaged
    {
        public static Vector4<T> Zero => new Vector4<T>(default);
        public static Vector4<T> One => new Vector4<T>(Primitive<T>.One);
        public static Vector4<T> UnitX => new Vector4<T>(Primitive<T>.One, default, default, default);
        public static Vector4<T> UnitY => new Vector4<T>(default, Primitive<T>.One, default, default);
        public static Vector4<T> UnitZ => new Vector4<T>(default, default, Primitive<T>.One, default);
        public static Vector4<T> UnitW => new Vector4<T>(default, default, default, Primitive<T>.One);

        public T X { get; }
        public T Y { get; }
        public T Z { get; }
        public T W { get; }

        public Vector4(T xyzw)
        {
            X = xyzw;
            Y = xyzw;
            Z = xyzw;
            W = xyzw;
        }

        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithX(T x) => new Vector4<T>(x, Y, Z, W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithY(T y) => new Vector4<T>(X, y, Z, W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithZ(T z) => new Vector4<T>(X, Y, z, W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithW(T w) => new Vector4<T>(X, Y, Z, w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<TTo> Convert<TTo>() where TTo : unmanaged =>
            new Vector4<TTo>(
                Primitive<T>.Convert<TTo>(X),
                Primitive<T>.Convert<TTo>(Y),
                Primitive<T>.Convert<TTo>(Z),
                Primitive<T>.Convert<TTo>(W));


        #region `Object` Overrides

        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override bool Equals(object? obj) => obj is Vector4<T> other && Equals(other);
        public override string ToString() => $"<{X}, {Y}, {Z}, {W}>";

        #endregion


        #region IEquatable

        public bool Equals(Vector4<T> other) => Vector.All(this == other);

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
            builder.Append(separator);
            builder.Append(' ');
            builder.Append((W as IFormattable)!.ToString(format, formatProvider));
            builder.Append('>');
            return builder.ToString();
        }

        #endregion


        #region Operators

        #region Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator ==(Vector4<T> a, T b) => a == new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator ==(T a, Vector4<T> b) => new Vector4<T>(a) == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator ==(Vector4<T> a, Vector4<T> b) => Equals(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(Vector4<T> a, T b) => a != new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(T a, Vector4<T> b) => new Vector4<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(Vector4<T> a, Vector4<T> b) => NotEquals(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(Vector4<T> a, T b) => a + new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(T a, Vector4<T> b) => new Vector4<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(Vector4<T> a, Vector4<T> b) => Add(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(Vector4<T> a, T b) => a - new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(T a, Vector4<T> b) => new Vector4<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(Vector4<T> a, Vector4<T> b) => Subtract(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(Vector4<T> a, T b) => a * new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(T a, Vector4<T> b) => new Vector4<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(Vector4<T> a, Vector4<T> b) => Multiply(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(Vector4<T> a, T b) => a / new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(T a, Vector4<T> b) => new Vector4<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(Vector4<T> a, Vector4<T> b) => Divide(a, b);

        #endregion


        #region And

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator &(Vector4<T> a, T b) => a & new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator &(T a, Vector4<T> b) => new Vector4<T>(a) & b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator &(Vector4<T> a, Vector4<T> b) => And(a, b);

        #endregion


        #region Or

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator |(Vector4<T> a, T b) => a | new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator |(T a, Vector4<T> b) => new Vector4<T>(a) | b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator |(Vector4<T> a, Vector4<T> b) => Or(a, b);

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(Vector4<T> a, T b) => a > new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(T a, Vector4<T> b) => new Vector4<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(Vector4<T> a, Vector4<T> b) => GreaterThan(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(Vector4<T> a, T b) => a < new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(T a, Vector4<T> b) => new Vector4<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(Vector4<T> a, Vector4<T> b) => LessThan(a, b);

        #endregion


        #region Greater Than Or Equal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >=(Vector4<T> a, T b) => a > new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >=(T a, Vector4<T> b) => new Vector4<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >=(Vector4<T> a, Vector4<T> b) => GreaterThanOrEqual(a, b);

        #endregion


        #region Less Than Or Equal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <=(Vector4<T> a, T b) => a < new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <=(T a, Vector4<T> b) => new Vector4<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <=(Vector4<T> a, Vector4<T> b) => LessThanOrEqual(a, b);

        #endregion

        #endregion
    }
}
