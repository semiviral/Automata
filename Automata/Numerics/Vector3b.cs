#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

#endregion

namespace Automata.Numerics
{
    public readonly partial struct Vector3b
    {
        private readonly int _X;
        private readonly int _Y;
        private readonly int _Z;
        private readonly int _W;

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

        public unsafe Vector3b(bool value)
        {
            int intValue = -(*(int*)&value);

            (_X, _Y, _Z, _W) = (intValue, intValue, intValue, VectorConstants.INTEGER_BOOLEAN_FALSE_VALUE);
        }

        public unsafe Vector3b(bool x, bool y, bool z) =>
            (_X, _Y, _Z, _W) = (-(*(int*)&x), -(*(int*)&y), -(*(int*)&z), VectorConstants.INTEGER_BOOLEAN_FALSE_VALUE);

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

        public static Vector3b operator ==(Vector3b a, Vector3b b) => (Vector3b)VectorConstants.EqualsImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3b operator !=(Vector3b a, Vector3b b) => !(Vector3b)VectorConstants.EqualsImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3b operator !(Vector3b a) => new Vector3b(!a.X, !a.Y, !a.Z);

        public static unsafe explicit operator Vector128<int>(Vector3b a) => Sse2.LoadVector128((int*)&a);
        public static unsafe explicit operator Vector3b(Vector128<int> a) => *(Vector3b*)&a;
    }
}
