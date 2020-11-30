using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable CognitiveComplexity
// ReSharper disable ConvertToAutoProperty

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2<T> : IEquatable<Vector2<T>>, IFormattable where T : unmanaged
    {
        public static Vector2<T> Zero => new Vector2<T>(default);
        public static Vector2<T> One => new Vector2<T>(Primitive<T>.One);
        public static Vector2<T> UnitX => new Vector2<T>(Primitive<T>.One, default);
        public static Vector2<T> UnitY => new Vector2<T>(default, Primitive<T>.One);

        public T X { get; }
        public T Y { get; }

        public Vector2(T xy)
        {
            X = xy;
            Y = xy;
        }

        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2<T> WithX(T x) => new Vector2<T>(x, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2<T> WithY(T y) => new Vector2<T>(X, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2<TTo> Convert<TTo>() where TTo : unmanaged
        {
            dynamic temp = this;
            return new Vector2<TTo>((TTo)temp.X, (TTo)temp.Y);
        }


        #region `Object` Overrides

        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override bool Equals(object? obj) => obj is Vector2<T> other && Equals(other);
        public override string ToString() => $"<{X}, {Y}>";

        #endregion


        #region IEquatable

        public bool Equals(Vector2<T> other) => Vector.All(this == other);

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
            builder.Append('>');
            return builder.ToString();
        }

        #endregion


        #region Operators

        #region Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator ==(Vector2<T> a, T b) => a == new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator ==(T a, Vector2<T> b) => new Vector2<T>(a) == b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator ==(Vector2<T> a, Vector2<T> b) => Equals(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator !=(Vector2<T> a, T b) => a != new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator !=(T a, Vector2<T> b) => new Vector2<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator !=(Vector2<T> a, Vector2<T> b) => NotEquals(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(Vector2<T> a, T b) => a + new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(T a, Vector2<T> b) => new Vector2<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator +(Vector2<T> a, Vector2<T> b) => Add(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(Vector2<T> a, T b) => a - new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(T a, Vector2<T> b) => new Vector2<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator -(Vector2<T> a, Vector2<T> b) => Subtract(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(Vector2<T> a, T b) => a * new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(T a, Vector2<T> b) => new Vector2<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator *(Vector2<T> a, Vector2<T> b) => Multiply(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(Vector2<T> a, T b) => a / new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(T a, Vector2<T> b) => new Vector2<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator /(Vector2<T> a, Vector2<T> b) => Divide(a, b);

        #endregion


        #region And

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator &(Vector2<T> a, T b) => a & new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator &(T a, Vector2<T> b) => new Vector2<T>(a) & b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator &(Vector2<T> a, Vector2<T> b) => And(a, b);

        #endregion


        #region Or

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator |(Vector2<T> a, T b) => a | new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator |(T a, Vector2<T> b) => new Vector2<T>(a) | b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> operator |(Vector2<T> a, Vector2<T> b) => Or(a, b);

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(Vector2<T> a, T b) => a > new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(T a, Vector2<T> b) => new Vector2<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >(Vector2<T> a, Vector2<T> b) => GreaterThan(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(Vector2<T> a, T b) => a < new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(T a, Vector2<T> b) => new Vector2<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <(Vector2<T> a, Vector2<T> b) => LessThan(a, b);

        #endregion


        #region Greater Than Or Equal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >=(Vector2<T> a, T b) => a >= new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >=(T a, Vector2<T> b) => new Vector2<T>(a) >= b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator >=(Vector2<T> a, Vector2<T> b) => GreaterThanOrEqual(a, b);

        #endregion


        #region Less Than Or Equal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <=(Vector2<T> a, T b) => a < new Vector2<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <=(T a, Vector2<T> b) => new Vector2<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> operator <=(Vector2<T> a, Vector2<T> b) => LessThanOrEqual(a, b);

        #endregion

        #endregion
    }
}
