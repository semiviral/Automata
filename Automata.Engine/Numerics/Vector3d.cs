#region

using System;
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
    public readonly partial struct Vector3d : IEquatable<Vector3d>
    {
        public static Vector3d Zero { get; } = new Vector3d(0d);
        public static Vector3d One { get; } = new Vector3d(1d);
        public static Vector3d UnitX { get; } = new Vector3d(1d, 0d, 0d);
        public static Vector3d UnitY { get; } = new Vector3d(0d, 1d, 0d);
        public static Vector3d UnitZ { get; } = new Vector3d(0d, 0d, 1d);

        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public double this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public Vector3d(double value) => (X, Y, Z) = (value, value, value);
        public Vector3d(double x, double y) => (X, Y, Z) = (x, y, 0d);
        public Vector3d(double x, double y, double z) => (X, Y, Z) = (x, y, z);

        public override bool Equals(object? obj) => obj is Vector3d other && Equals(other);
        public bool Equals(Vector3d other) => Vector3b.All(this == other);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public override string ToString() => string.Format(FormatHelper.VECTOR_3_COMPONENT, nameof(Vector3d), X, Y, Z);

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
