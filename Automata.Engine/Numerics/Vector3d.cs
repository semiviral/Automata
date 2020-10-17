#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Global

#endregion

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3d
    {
        #region Fields / Properties

        public static Vector3d Zero { get; } = new Vector3d(0d);
        public static Vector3d One { get; } = new Vector3d(1d);
        public static Vector3d UnitX { get; } = new Vector3d(1d, 0d, 0d);
        public static Vector3d UnitY { get; } = new Vector3d(0d, 1d, 0d);
        public static Vector3d UnitZ { get; } = new Vector3d(0d, 0d, 1d);

        public readonly double X;
        public readonly double Y;
        public readonly double Z;
        public readonly double W;

        public double this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public Vector3d(double value) => (X, Y, Z, W) = (value, value, value, value);
        public Vector3d(double x, double y) => (X, Y, Z, W) = (x, y, 0d, 0d);
        public Vector3d(double x, double y, double z) => (X, Y, Z, W) = (x, y, z, 0d);

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

        public override string ToString() => string.Format(FormatHelper.VECTOR_3_COMPONENT, nameof(Vector3d), X, Y, Z);

        #endregion


        #region Operators

        public static Vector3b operator ==(Vector3d a, Vector3d b) => EqualsImpl(a, b);
        public static Vector3b operator ==(Vector3d a, double b) => EqualsImpl(a, b);
        public static Vector3b operator ==(double a, Vector3d b) => EqualsImpl(a, b);

        public static Vector3b operator !=(Vector3d a, Vector3d b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(Vector3d a, double b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(double a, Vector3d b) => NotEqualsImpl(a, b);

        public static Vector3d operator +(Vector3d a, Vector3d b) => AddImpl(a, b);
        public static Vector3d operator +(Vector3d a, double b) => AddImpl(a, b);
        public static Vector3d operator +(double a, Vector3d b) => AddImpl(a, b);

        public static Vector3d operator -(Vector3d a, Vector3d b) => SubtractImpl(a, b);
        public static Vector3d operator -(Vector3d a, double b) => SubtractImpl(a, b);
        public static Vector3d operator -(double a, Vector3d b) => SubtractImpl(a, b);

        public static Vector3d operator *(Vector3d a, Vector3d b) => MultiplyImpl(a, b);
        public static Vector3d operator *(Vector3d a, double b) => MultiplyImpl(a, b);
        public static Vector3d operator *(double a, Vector3d b) => MultiplyImpl(a, b);

        public static Vector3d operator /(Vector3d a, Vector3d b) => DivideImpl(a, b);
        public static Vector3d operator /(Vector3d a, double b) => DivideImpl(a, b);
        public static Vector3d operator /(double a, Vector3d b) => DivideImpl(a, b);

        public static Vector3b operator >(Vector3d a, Vector3d b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(Vector3d a, double b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(double a, Vector3d b) => GreaterThanImpl(a, b);

        public static Vector3b operator <(Vector3d a, Vector3d b) => LessThanImpl(a, b);
        public static Vector3b operator <(Vector3d a, double b) => LessThanImpl(a, b);
        public static Vector3b operator <(double a, Vector3d b) => LessThanImpl(a, b);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector3d(Vector256<double> a) => *(Vector3d*)&a;
        public static unsafe explicit operator Vector256<double>(Vector3d a) => Avx.LoadVector256((double*)&a);

        #endregion
    }
}
