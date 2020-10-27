using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2ui : IEquatable<Vector2ui>
    {
        public static Vector2ui Zero { get; } = new Vector2ui(0);
        public static Vector2ui One { get; } = new Vector2ui(1);
        public static Vector2ui UnitX { get; } = new Vector2ui(1, 0);
        public static Vector2ui UnitY { get; } = new Vector2ui(0, 1);

        public readonly int X;
        public readonly int Y;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public Vector2ui(int xy) => (X, Y) = (xy, xy);
        public Vector2ui(int x, int y) => (X, Y) = (x, y);

        public override bool Equals(object? obj) => obj is Vector2ui other && Equals(other);
        public bool Equals(Vector2ui other) => Vector2b.All(this == other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => string.Format(FormatHelper.VECTOR_2_COMPONENT, nameof(Vector2ui), X, Y);


        #region Operators

        public static Vector2b operator ==(Vector2ui a, Vector2ui b) => EqualsImpl(a, b);
        public static Vector2b operator ==(Vector2ui a, int b) => EqualsImpl(a, b);
        public static Vector2b operator ==(int a, Vector2ui b) => EqualsImpl(a, b);

        public static Vector2b operator !=(Vector2ui a, Vector2ui b) => !EqualsImpl(a, b);
        public static Vector2b operator !=(Vector2ui a, int b) => !EqualsImpl(a, b);
        public static Vector2b operator !=(int a, Vector2ui b) => !EqualsImpl(a, b);

        public static Vector2ui operator &(Vector2ui a, Vector2ui b) => BitwiseAndImpl(a, b);
        public static Vector2ui operator &(Vector2ui a, int b) => BitwiseAndImpl(a, b);
        public static Vector2ui operator &(int a, Vector2ui b) => BitwiseAndImpl(a, b);

        public static Vector2ui operator |(Vector2ui a, Vector2ui b) => BitwiseOrImpl(a, b);
        public static Vector2ui operator |(Vector2ui a, int b) => BitwiseOrImpl(a, b);
        public static Vector2ui operator |(int a, Vector2ui b) => BitwiseOrImpl(a, b);

        public static Vector2ui operator +(Vector2ui a, Vector2ui b) => AddImpl(a, b);
        public static Vector2ui operator +(Vector2ui a, int b) => AddImpl(a, b);
        public static Vector2ui operator +(int a, Vector2ui b) => AddImpl(a, b);

        public static Vector2ui operator -(Vector2ui a, Vector2ui b) => SubtractImpl(a, b);
        public static Vector2ui operator -(Vector2ui a, int b) => SubtractImpl(a, b);
        public static Vector2ui operator -(int a, Vector2ui b) => SubtractImpl(a, b);

        public static Vector2ui operator *(Vector2ui a, Vector2ui b) => MultiplyImpl(a, b);
        public static Vector2ui operator *(Vector2ui a, int b) => MultiplyImpl(a, b);
        public static Vector2ui operator *(int a, Vector2ui b) => MultiplyImpl(a, b);

        public static Vector2ui operator /(Vector2ui a, Vector2ui b) => DivideImpl(a, b);
        public static Vector2ui operator /(Vector2ui a, int b) => DivideImpl(a, b);
        public static Vector2ui operator /(int a, Vector2ui b) => DivideImpl(a, b);

        public static Vector2b operator >(Vector2ui a, Vector2ui b) => GreaterThanImpl(a, b);
        public static Vector2b operator >(Vector2ui a, int b) => GreaterThanImpl(a, b);
        public static Vector2b operator >(int a, Vector2ui b) => GreaterThanImpl(a, b);

        public static Vector2b operator <(Vector2ui a, Vector2ui b) => LessThanImpl(a, b);
        public static Vector2b operator <(Vector2ui a, int b) => LessThanImpl(a, b);
        public static Vector2b operator <(int a, Vector2ui b) => LessThanImpl(a, b);

        #endregion


        #region Conversion Operators

        public static explicit operator Vector128<int>(Vector2ui a) => Unsafe.As<Vector2ui, Vector128<int>>(ref a);
        public static explicit operator Vector2ui(Vector128<int> a) => Unsafe.As<Vector128<int>, Vector2ui>(ref a);

        public static explicit operator Vector2(Vector2ui a) => new Vector2(a.X, a.Y);

        #endregion
    }
}
