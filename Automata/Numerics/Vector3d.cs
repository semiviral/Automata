#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Global

#endregion

namespace Automata.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3d
    {
        #region Members

        public static Vector3d Zero { get; } = new Vector3d(0d);
        public static Vector3d One { get; } = new Vector3d(1d);

        private readonly double _X;
        private readonly double _Y;
        private readonly double _Z;
        private readonly double _W;

        public double X => _X;
        public double Y => _Y;
        public double Z => _Z;

        public double this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public Vector3d(double value) => (_X, _Y, _Z, _W) = (value, value, value, value);
        public Vector3d(double x, double y) => (_X, _Y, _Z, _W) = (x, y, 0d, 0d);
        public Vector3d(double x, double y, double z) => (_X, _Y, _Z, _W) = (x, y, z, 0d);

        #endregion

        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector3d a)
            {
                return Vector3b.All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        #endregion


        #region Operators

        public static Vector3b operator ==(Vector3d a, Vector3d b) => EqualsImpl(a, b);
        public static Vector3b operator ==(Vector3d a, int b) => EqualsImpl(a, b);
        public static Vector3b operator ==(int a, Vector3d b) => EqualsImpl(a, b);

        public static Vector3b operator !=(Vector3d a, Vector3d b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(Vector3d a, int b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(int a, Vector3d b) => NotEqualsImpl(a, b);

        public static Vector3d operator +(Vector3d a, Vector3d b) => AddImpl(a, b);
        public static Vector3d operator +(Vector3d a, int b) => AddImpl(a, b);
        public static Vector3d operator +(int a, Vector3d b) => AddImpl(a, b);

        public static Vector3d operator -(Vector3d a, Vector3d b) => SubtractImpl(a, b);
        public static Vector3d operator -(Vector3d a, int b) => SubtractImpl(a, b);
        public static Vector3d operator -(int a, Vector3d b) => SubtractImpl(a, b);

        public static Vector3d operator *(Vector3d a, Vector3d b) => MultiplyImpl(a, b);
        public static Vector3d operator *(Vector3d a, int b) => MultiplyImpl(a, b);
        public static Vector3d operator *(int a, Vector3d b) => MultiplyImpl(a, b);

        public static Vector3d operator /(Vector3d a, Vector3d b) => DivideImpl(a, b);
        public static Vector3d operator /(Vector3d a, int b) => DivideImpl(a, b);
        public static Vector3d operator /(int a, Vector3d b) => DivideImpl(a, b);

        public static Vector3b operator >(Vector3d a, Vector3d b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(Vector3d a, int b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(int a, Vector3d b) => GreaterThanImpl(a, b);

        public static Vector3b operator <(Vector3d a, Vector3d b) => LessThanImpl(a, b);
        public static Vector3b operator <(Vector3d a, int b) => LessThanImpl(a, b);
        public static Vector3b operator <(int a, Vector3d b) => LessThanImpl(a, b);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector3d(Vector256<double> a) => *(Vector3d*)&a;
        public static unsafe explicit operator Vector256<double>(Vector3d a) => Avx.LoadAlignedVector256((double*)&a);

        #endregion
    }
}
