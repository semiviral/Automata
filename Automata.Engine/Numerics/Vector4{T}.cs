using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Automata.Engine.Numerics
{
    public readonly struct Vector4<T> where T : unmanaged
    {
        public static Vector4<T> Zero => new Vector4<T>(default);
        public static Vector4<T> One => new Vector4<T>(Primitive<T>.One);
        public static Vector4<T> UnitX => new Vector4<T>(Primitive<T>.One, default, default, default);
        public static Vector4<T> UnitY => new Vector4<T>(default, Primitive<T>.One, default, default);
        public static Vector4<T> UnitZ => new Vector4<T>(default, default, Primitive<T>.One, default);
        public static Vector4<T> UnitW => new Vector4<T>(default, default, default, Primitive<T>.One);

        private readonly T _X;
        private readonly T _Y;
        private readonly T _Z;
        private readonly T _W;

        public T X => _X;
        public T Y => _Y;
        public T Z => _Z;
        public T W => _W;

        public Vector4(T xyzw)
        {
            _X = xyzw;
            _Y = xyzw;
            _Z = xyzw;
            _W = xyzw;
        }

        public Vector4(T x, T y, T z, T w)
        {
            _X = x;
            _Y = y;
            _Z = z;
            _W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithX(T x) => new Vector4<T>(x, _Y, _Z, _W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithY(T y) => new Vector4<T>(_X, y, _Z, _W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithZ(T z) => new Vector4<T>(_X, _Y, z, _W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<T> WithW(T w) => new Vector4<T>(_X, _Y, _Z, w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4<TTo> Convert<TTo>() where TTo : unmanaged => new Vector4<TTo>((TTo)(object)_X, (TTo)(object)_Y, (TTo)(object)_Z, (TTo)(object)_W);


        #region `Object` Overrides

        public override int GetHashCode() => HashCode.Combine(_X, _Y);
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
        public static Vector4<bool> operator ==(Vector4<T> a, Vector4<T> b) => Vector.EqualsInternal(a, b);

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(Vector4<T> a, T b) => a != new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(T a, Vector4<T> b) => new Vector4<T>(a) != b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator !=(Vector4<T> a, Vector4<T> b) => Vector.NotEqualsInternal(a, b);

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(Vector4<T> a, T b) => a + new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(T a, Vector4<T> b) => new Vector4<T>(a) + b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator +(Vector4<T> a, Vector4<T> b) => Vector.AddInternal(a, b);

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(Vector4<T> a, T b) => a - new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(T a, Vector4<T> b) => new Vector4<T>(a) - b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator -(Vector4<T> a, Vector4<T> b) => Vector.SubtractInternal(a, b);

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(Vector4<T> a, T b) => a * new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(T a, Vector4<T> b) => new Vector4<T>(a) * b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator *(Vector4<T> a, Vector4<T> b) => Vector.MultiplyInternal(a, b);

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(Vector4<T> a, T b) => a / new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(T a, Vector4<T> b) => new Vector4<T>(a) / b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> operator /(Vector4<T> a, Vector4<T> b) => Vector.DivideInternal(a, b);

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(Vector4<T> a, T b) => a > new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(T a, Vector4<T> b) => new Vector4<T>(a) > b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator >(Vector4<T> a, Vector4<T> b) => Vector.GreaterThanInternal(a, b);

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(Vector4<T> a, T b) => a < new Vector4<T>(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(T a, Vector4<T> b) => new Vector4<T>(a) < b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> operator <(Vector4<T> a, Vector4<T> b) => Vector.LessThanInternal(a, b);

        #endregion

        #endregion
    }
}
