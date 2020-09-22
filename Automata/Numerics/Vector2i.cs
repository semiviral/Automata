#region

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable ConvertToAutoProperty
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

namespace Automata.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2i
    {
        #region Fields / Properties

        public static Vector2i Zero { get; } = new Vector2i(0);
        public static Vector2i One { get; } = new Vector2i(1);

        private readonly int _X;
        private readonly int _Y;

        public int X => _X;
        public int Y => _Y;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public Vector2i(int xy) => (_X, _Y) = (xy, xy);
        public Vector2i(int x, int y) => (_X, _Y) = (x, y);

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector2i a)
            {
                return Vector2b.All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        #endregion


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
        public static explicit operator Vector2i(Size a) => Unsafe.As<Size, Vector2i>(ref a);

        public static unsafe explicit operator Vector128<int>(Vector2i a) => Sse2.LoadVector128((int*)&a);

        public static explicit operator Vector2(Vector2i a) => new Vector2(a.X, a.Y);

        #endregion
    }
}
