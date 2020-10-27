#region

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

#endregion


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3i : IEquatable<Vector3i>
    {
        public static Vector3i Zero { get; } = new Vector3i(0);
        public static Vector3i One { get; } = new Vector3i(1);
        public static Vector3i UnitX { get; } = new Vector3i(1, 0, 0);
        public static Vector3i UnitY { get; } = new Vector3i(0, 1, 0);
        public static Vector3i UnitZ { get; } = new Vector3i(0, 0, 1);

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public Vector3i(int xyz) => (X, Y, Z) = (xyz, xyz, xyz);
        public Vector3i(int x, int y, int z) => (X, Y, Z) = (x, y, z);

        public override bool Equals(object? obj) => obj is Vector3i other && Equals(other);
        public bool Equals(Vector3i other) => Vector3b.All(this == other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override string ToString() => string.Format(FormatHelper.VECTOR_3_COMPONENT, nameof(Vector3i), X, Y, Z);


        #region Operators

        public static Vector3b operator ==(Vector3i a, Vector3i b) => EqualsImpl(a, b);
        public static Vector3b operator ==(Vector3i a, int b) => EqualsImpl(a, b);
        public static Vector3b operator ==(int a, Vector3i b) => EqualsImpl(a, b);

        public static Vector3b operator !=(Vector3i a, Vector3i b) => !EqualsImpl(a, b);
        public static Vector3b operator !=(Vector3i a, int b) => !EqualsImpl(a, b);
        public static Vector3b operator !=(int a, Vector3i b) => !EqualsImpl(a, b);

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

        public static Vector3i operator %(Vector3i a, Vector3i b) => RemainderImpl(a, b);
        public static Vector3i operator %(Vector3i a, int b) => RemainderImpl(a, b);
        public static Vector3i operator %(int a, Vector3i b) => RemainderImpl(a, b);

        public static Vector3b operator >(Vector3i a, Vector3i b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(Vector3i a, int b) => GreaterThanImpl(a, b);
        public static Vector3b operator >(int a, Vector3i b) => GreaterThanImpl(a, b);

        public static Vector3b operator <(Vector3i a, Vector3i b) => LessThanImpl(a, b);
        public static Vector3b operator <(Vector3i a, int b) => LessThanImpl(a, b);
        public static Vector3b operator <(int a, Vector3i b) => LessThanImpl(a, b);

        public static Vector3b operator >=(Vector3i a, Vector3i b) => GreaterThanImpl(a, b) | EqualsImpl(a, b);
        public static Vector3b operator >=(Vector3i a, int b) => GreaterThanImpl(a, b) | EqualsImpl(a, b);
        public static Vector3b operator >=(int a, Vector3i b) => GreaterThanImpl(a, b) | EqualsImpl(a, b);

        public static Vector3b operator <=(Vector3i a, Vector3i b) => LessThanImpl(a, b) | EqualsImpl(a, b);
        public static Vector3b operator <=(Vector3i a, int b) => LessThanImpl(a, b) | EqualsImpl(a, b);
        public static Vector3b operator <=(int a, Vector3i b) => LessThanImpl(a, b) | EqualsImpl(a, b);

        #endregion


        #region Conversions

        public static explicit operator Vector3i(Vector128<int> a) => Unsafe.As<Vector128<int>, Vector3i>(ref a);
        public static explicit operator Vector3i(Vector128<uint> a) => Unsafe.As<Vector128<uint>, Vector3i>(ref a);
        public static explicit operator Vector128<int>(Vector3i a) => Unsafe.As<Vector3i, Vector128<int>>(ref a);

        public static implicit operator Vector3(Vector3i a) => new Vector3(a.X, a.Y, a.Z);
        public static implicit operator Vector3i((int, int, int) valueTuple) => Unsafe.As<(int, int, int), Vector3i>(ref valueTuple);

        #endregion
    }
}
