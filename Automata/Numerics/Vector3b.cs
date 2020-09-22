#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace Automata.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector3b
    {
        #region Fields / Properties

        private readonly byte _X;
        private readonly byte _Y;
        private readonly byte _Z;

        public bool X => _X == VectorConstants.INTEGER_BOOLEAN_TRUE_VALUE;
        public bool Y => _Y == VectorConstants.INTEGER_BOOLEAN_TRUE_VALUE;
        public bool Z => _Z == VectorConstants.INTEGER_BOOLEAN_TRUE_VALUE;

        public bool this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public Vector3b(bool value)
        {
            byte numericValue = AutomataMath.BoolToByte(value);

            (_X, _Y, _Z) = (numericValue, numericValue, numericValue);
        }

        public Vector3b(bool x, bool y, bool z) =>
            (_X, _Y, _Z) = (AutomataMath.BoolToByte(x), AutomataMath.BoolToByte(y), AutomataMath.BoolToByte(z));

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector3b a)
            {
                return All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode() ^ _Z.GetHashCode();

        #endregion


        #region Operators

        public static Vector3b operator ==(Vector3b a, Vector3b b) => EqualsImpl(a, b);
        public static Vector3b operator !=(Vector3b a, Vector3b b) => NotEqualsImpl(a, b);
        public static Vector3b operator !(Vector3b a) => new Vector3b(!a.X, !a.Y, !a.Z);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector128<int>(Vector3b a) => Sse2.LoadVector128((int*)&a);
        public static unsafe explicit operator Vector3b(Vector128<int> a) => *(Vector3b*)&a;
        public static explicit operator Vector3b(Vector256<double> a) => (Vector3b)Avx.ConvertToVector128Int32(a);

        #endregion
    }
}
