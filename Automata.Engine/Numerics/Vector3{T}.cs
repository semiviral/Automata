using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3<T> : IEquatable<Vector3<T>>, IFormattable where T : unmanaged
    {
        public static Vector3<T> Zero => new Vector3<T>(default);
        public static Vector3<T> One => new Vector3<T>(Primitive<T>.One);
        public static Vector3<T> UnitX => new Vector3<T>(Primitive<T>.One, default, default);
        public static Vector3<T> UnitY => new Vector3<T>(default, Primitive<T>.One, default);
        public static Vector3<T> UnitZ => new Vector3<T>(default, default, Primitive<T>.One);

        public T X { get; }
        public T Y { get; }
        public T Z { get; }

        public Vector3(T xyz)
        {
            X = xyz;
            Y = xyz;
            Z = xyz;
        }

        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3<T> WithX(T x) => new Vector3<T>(x, Y, Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3<T> WithY(T y) => new Vector3<T>(X, y, Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3<T> WithZ(T z) => new Vector3<T>(X, Y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3<TTo> Convert<TTo>() where TTo : unmanaged => new Vector3<TTo>((TTo)(object)X, (TTo)(object)Y, (TTo)(object)Z);


        #region `Object` Overrides

        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override bool Equals(object? obj) => obj is Vector3<T> other && Equals(other);
        public override string ToString() => $"<{X}, {Y}, {Z}>";

        #endregion


        #region IEquatable

        public bool Equals(Vector3<T> other) => Vector.All(this == other);

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
        public static Vector3<bool> operator ==(Vector3<T> a, T b) => a == new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator ==(T a, Vector3<T> b) => new Vector3<T>(a) == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator ==(Vector3<T> a, Vector3<T> b) => Equals(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator !=(Vector3<T> a, T b) => a != new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator !=(T a, Vector3<T> b) => new Vector3<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator !=(Vector3<T> a, Vector3<T> b) => NotEquals(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(Vector3<T> a, T b) => a + new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(T a, Vector3<T> b) => new Vector3<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator +(Vector3<T> a, Vector3<T> b) => Add(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(Vector3<T> a, T b) => a - new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(T a, Vector3<T> b) => new Vector3<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator -(Vector3<T> a, Vector3<T> b) => Subtract(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(Vector3<T> a, T b) => a * new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(T a, Vector3<T> b) => new Vector3<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator *(Vector3<T> a, Vector3<T> b) => Multiply(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(Vector3<T> a, T b) => a / new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(T a, Vector3<T> b) => new Vector3<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator /(Vector3<T> a, Vector3<T> b) => Divide(a, b);

        #endregion

        #region And

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator &(Vector3<T> a, T b) => a & new Vector3<T>(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator &(T a, Vector3<T> b) => new Vector3<T>(a) & b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator &(Vector3<T> a, Vector3<T> b) => And(a, b);

        #endregion


        #region Or

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator |(Vector3<T> a, T b) => a | new Vector3<T>(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator |(T a, Vector3<T> b) => new Vector3<T>(a) | b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> operator |(Vector3<T> a, Vector3<T> b) => Or(a, b);

        #endregion

        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(Vector3<T> a, T b) => a > new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(T a, Vector3<T> b) => new Vector3<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >(Vector3<T> a, Vector3<T> b) => GreaterThan(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(Vector3<T> a, T b) => a < new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(T a, Vector3<T> b) => new Vector3<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <(Vector3<T> a, Vector3<T> b) => LessThan(a, b);

        #endregion


        #region Greater Than Or Equal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >=(Vector3<T> a, T b) => a >= new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >=(T a, Vector3<T> b) => new Vector3<T>(a) >= b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator >=(Vector3<T> a, Vector3<T> b) => GreaterThanOrEqual(a, b);

        #endregion


        #region Less Than Or Equal

        // todo
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <=(Vector3<T> a, T b) => a < new Vector3<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <=(T a, Vector3<T> b) => new Vector3<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> operator <=(Vector3<T> a, Vector3<T> b) => LessThanOrEqual(a, b);


        #endregion

        #endregion
    }
}
