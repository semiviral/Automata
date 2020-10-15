#region

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

namespace Automata.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3i
    {
        #region Fields / Properties

        private static readonly string _ToStringFormat = $"{typeof(Vector3i)}({{0}}, {{1}}, {{2}})";

        public static Vector3i Zero { get; } = new Vector3i(0);
        public static Vector3i One { get; } = new Vector3i(1);

        private readonly int _X;
        private readonly int _Y;
        private readonly int _Z;

        public int X => _X;
        public int Y => _Y;
        public int Z => _Z;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public Vector3i(int xyz) => (_X, _Y, _Z) = (xyz, xyz, xyz);
        public Vector3i(int x, int y, int z) => (_X, _Y, _Z) = (x, y, z);

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector3i a)
            {
                return Vector3b.All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public override string ToString() => string.Format(_ToStringFormat, X, Y, Z);

        #endregion


        #region Operators

        public static Vector3b operator ==(Vector3i a, Vector3i b) => EqualsImpl(a, b);
        public static Vector3b operator ==(Vector3i a, int b) => EqualsImpl(a, b);
        public static Vector3b operator ==(int a, Vector3i b) => EqualsImpl(a, b);

        public static Vector3b operator !=(Vector3i a, Vector3i b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(Vector3i a, int b) => NotEqualsImpl(a, b);
        public static Vector3b operator !=(int a, Vector3i b) => NotEqualsImpl(a, b);

        public static Vector3i operator &(Vector3i a, Vector3i b) => BitwiseAndImpl(a, b);
        public static Vector3i operator &(Vector3i a, int b) => BitwiseAndImpl(a, b);
        public static Vector3i operator &(int a, Vector3i b) => BitwiseAndImpl(a, b);

        public static Vector3i operator |(Vector3i a, Vector3i b) => BitwiseOrImpl(a, b);
        public static Vector3i operator |(Vector3i a, int b) => BitwiseOrImpl(a, b);
        public static Vector3i operator |(int a, Vector3i b) => BitwiseOrImpl(a, b);

        public static Vector3i operator +(Vector3i a, Vector3i b) => AddImpl(a, b);
        public static Vector3i operator +(Vector3i a, int b) => AddImpl(a, b);
        public static Vector3i operator +(int a, Vector3i b) => AddImpl(a, b);

        public static Vector3i operator -(Vector3i a, Vector3i b) => SubtractImpl(a, b);
        public static Vector3i operator -(Vector3i a, int b) => SubtractImpl(a, b);
        public static Vector3i operator -(int a, Vector3i b) => SubtractImpl(a, b);

        public static Vector3i operator *(Vector3i a, Vector3i b) => MultiplyImpl(a, b);
        public static Vector3i operator *(Vector3i a, int b) => MultiplyImpl(a, b);
        public static Vector3i operator *(int a, Vector3i b) => MultiplyImpl(a, b);

        public static Vector3i operator /(Vector3i a, Vector3i b) => DivideImpl(a, b);
        public static Vector3i operator /(Vector3i a, int b) => DivideImpl(a, b);
        public static Vector3i operator /(int a, Vector3i b) => DivideImpl(a, b);

        public static Vector3b operator >(Vector3i a, Vector3i b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(Vector3i a, int b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(int a, Vector3i b) => GreaterThanImpl(a, b);

        public static Vector3b operator <(Vector3i a, Vector3i b) => LessThanImpl(a, b);
        public static Vector3b operator <(Vector3i a, int b) => LessThanImpl(a, b);
        public static Vector3b operator <(int a, Vector3i b) => LessThanImpl(a, b);

        public static Vector3b operator >=(Vector3i a, int b) => GreaterThanOrEqualImpl(a, b);
        public static Vector3b operator <=(Vector3i a, int b) => LessThanOrEqualImpl(a, b);

        #endregion


        #region Conversions

        public static explicit operator Vector3i(Vector128<int> a) => Unsafe.As<Vector128<int>, Vector3i>(ref a);
        public static explicit operator Vector3i(Vector128<uint> a) => Unsafe.As<Vector128<uint>, Vector3i>(ref a);
        public static explicit operator Vector128<int>(Vector3i a) => Unsafe.As<Vector3i, Vector128<int>>(ref a);
        public static implicit operator Vector3(Vector3i a) => new Vector3(a.X, a.Y, a.Z);

        #endregion
    }
}
