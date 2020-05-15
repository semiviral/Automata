#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace Automata.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Vector2b
    {
        #region Members

        private readonly int _X;
        private readonly int _Y;
        private readonly int _Z;
        private readonly int _W;

        public bool X => _X == VectorConstants.INTEGER_BOOLEAN_TRUE_VALUE;
        public bool Y => _Y == VectorConstants.INTEGER_BOOLEAN_TRUE_VALUE;

        public bool this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #endregion


        #region Constructors

        public unsafe Vector2b(bool value)
        {
            int intValue = -(*(int*)&value);

            (_X, _Y, _Z, _W) = (intValue, intValue, intValue, VectorConstants.INTEGER_BOOLEAN_FALSE_VALUE);
        }

        public unsafe Vector2b(bool x, bool y) =>
            (_X, _Y, _Z, _W) = (-(*(int*)&x), -(*(int*)&y), VectorConstants.INTEGER_BOOLEAN_FALSE_VALUE, VectorConstants.INTEGER_BOOLEAN_FALSE_VALUE);

        #endregion


        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj is Vector2b a)
            {
                return All(a == this);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode();

        #endregion


        #region Operators

        public static Vector2b operator ==(Vector2b a, Vector2b b) => EqualsImpl(a, b);
        public static Vector2b operator !=(Vector2b a, Vector2b b) => NotEqualsImpl(a, b);
        public static Vector2b operator !(Vector2b a) => new Vector2b(!a.X, !a.Y);

        #endregion


        #region Conversions

        public static unsafe explicit operator Vector128<int>(Vector2b a) => Sse2.LoadVector128((int*)&a);
        public static unsafe explicit operator Vector2b(Vector128<int> a) => *(Vector2b*)&a;

        #endregion
    }
}
