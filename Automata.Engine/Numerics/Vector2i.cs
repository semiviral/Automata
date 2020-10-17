#region

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2i : IEquatable<Vector2i>
    {
        public static Vector2i Zero { get; } = new Vector2i(0);
        public static Vector2i One { get; } = new Vector2i(1);
        public static Vector2i UnitX { get; } = new Vector2i(1, 0);
        public static Vector2i UnitY { get; } = new Vector2i(0, 1);

        public readonly int X;
        public readonly int Y;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public Vector2i(int xy) => (X, Y) = (xy, xy);
        public Vector2i(int x, int y) => (X, Y) = (x, y);

        public override bool Equals(object? obj) => obj is Vector2i other && Equals(other);
        public bool Equals(Vector2i other) => Vector2b.All(this == other);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public override string ToString() => string.Format(FormatHelper.VECTOR_2_COMPONENT, nameof(Vector2i), X, Y);

        #region Operators

        public static Vector2b operator ==(Vector2i a, Vector2i b) => EqualsImpl(a, b);
        public static Vector2b operator ==(Vector2i a, int b) => EqualsImpl(a, b);
        public static Vector2b operator ==(int a, Vector2i b) => EqualsImpl(a, b);

        public static Vector2b operator !=(Vector2i a, Vector2i b) => NotEqualsImpl(a, b);
        public static Vector2b operator !=(Vector2i a, int b) => NotEqualsImpl(a, b);
        public static Vector2b operator !=(int a, Vector2i b) => NotEqualsImpl(a, b);

        public static Vector2i operator &(Vector2i a, Vector2i b) => BitwiseAndImpl(a, b);
        public static Vector2i operator &(Vector2i a, int b) => BitwiseAndImpl(a, b);
        public static Vector2i operator &(int a, Vector2i b) => BitwiseAndImpl(a, b);

        public static Vector2i operator |(Vector2i a, Vector2i b) => BitwiseOrImpl(a, b);
        public static Vector2i operator |(Vector2i a, int b) => BitwiseOrImpl(a, b);
        public static Vector2i operator |(int a, Vector2i b) => BitwiseOrImpl(a, b);

        public static Vector2i operator +(Vector2i a, Vector2i b) => AddImpl(a, b);
        public static Vector2i operator +(Vector2i a, int b) => AddImpl(a, b);
        public static Vector2i operator +(int a, Vector2i b) => AddImpl(a, b);

        public static Vector2i operator -(Vector2i a, Vector2i b) => SubtractImpl(a, b);
        public static Vector2i operator -(Vector2i a, int b) => SubtractImpl(a, b);
        public static Vector2i operator -(int a, Vector2i b) => SubtractImpl(a, b);

        public static Vector2i operator *(Vector2i a, Vector2i b) => MultiplyImpl(a, b);
        public static Vector2i operator *(Vector2i a, int b) => MultiplyImpl(a, b);
        public static Vector2i operator *(int a, Vector2i b) => MultiplyImpl(a, b);

        public static Vector2b operator >(Vector2i a, Vector2i b) => GreaterThanImpl(a, b);
        public static Vector2b operator >(Vector2i a, int b) => GreaterThanImpl(a, b);
        public static Vector2b operator >(int a, Vector2i b) => GreaterThanImpl(a, b);

        public static Vector2b operator <(Vector2i a, Vector2i b) => LessThanImpl(a, b);
        public static Vector2b operator <(Vector2i a, int b) => LessThanImpl(a, b);
        public static Vector2b operator <(int a, Vector2i b) => LessThanImpl(a, b);

        #endregion

        #region Conversion Operators

        public static explicit operator Vector2i(Vector128<int> a) => Unsafe.As<Vector128<int>, Vector2i>(ref a);
        public static explicit operator Vector2i(Point a) => Unsafe.As<Point, Vector2i>(ref a);
        public static explicit operator Point(Vector2i a) => Unsafe.As<Vector2i, Point>(ref a);
        public static explicit operator Vector2i(Size a) => Unsafe.As<Size, Vector2i>(ref a);
        public static explicit operator Size(Vector2i a) => Unsafe.As<Vector2i, Size>(ref a);

        public static unsafe explicit operator Vector128<int>(Vector2i a) => Sse2.LoadVector128((int*)&a);

        public static explicit operator Vector2(Vector2i a) => new Vector2(a.X, a.Y);

        #endregion
    }
}
