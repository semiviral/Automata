#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

#endregion

namespace Automata.Numerics
{
    public readonly partial struct Vector2i
    {
        public static Vector2i Zero { get; } = new Vector2i(0);
        public static Vector2i One { get; } = new Vector2i(1);

        private readonly int _X;
        private readonly int _Y;
        private readonly int _Z;
        private readonly int _W;

        public int X => _X;
        public int Y => _Y;

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #region Constructors

        public Vector2i(int xyz) => (_X, _Y, _Z, _W) = (xyz, xyz, xyz, 0);

        public Vector2i(int x, int y) => (_X, _Y, _Z, _W) = (x, y, 0, 0);

        public Vector2i(int x, int y, int z) => (_X, _Y, _Z, _W) = (x, y, z, 0);

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

        public override int GetHashCode() => _X.GetHashCode() ^ _Y.GetHashCode();

        #endregion


        #region Operators

        public static Vector2b operator ==(Vector2i a, Vector2i b) => (Vector2b)VectorConstants.EqualsImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2b operator !=(Vector2i a, Vector2i b) => !(Vector2b)VectorConstants.EqualsImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector2i operator &(Vector2i a, Vector2i b) => (Vector2i)VectorConstants.BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator |(Vector2i a, Vector2i b) => (Vector2i)VectorConstants.BitwiseOrImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector2i operator +(Vector2i a, Vector2i b) => (Vector2i)VectorConstants.AddImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator +(Vector2i a, int b) => (Vector2i)VectorConstants.AddImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator +(int a, Vector2i b) => (Vector2i)VectorConstants.AddImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2i operator -(Vector2i a, Vector2i b) => (Vector2i)VectorConstants.SubtractImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator -(Vector2i a, int b) => (Vector2i)VectorConstants.SubtractImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator -(int a, Vector2i b) => (Vector2i)VectorConstants.SubtractImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2i operator *(Vector2i a, Vector2i b) => (Vector2i)VectorConstants.MultiplyImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator *(Vector2i a, int b) => (Vector2i)VectorConstants.MultiplyImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator *(int a, Vector2i b) => (Vector2i)VectorConstants.MultiplyImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2b operator >(Vector2i a, Vector2i b) => (Vector2b)VectorConstants.GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2b operator <(Vector2i a, Vector2i b) => (Vector2b)VectorConstants.LessThanImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector2b operator >(Vector2i a, int b) => (Vector2b)VectorConstants.GreaterThanImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2b operator <(Vector2i a, int b) => (Vector2b)VectorConstants.LessThanImpl((Vector128<int>)a, Vector128.Create(b));

        public static Vector2b operator >(int a, Vector2i b) => (Vector2b)VectorConstants.GreaterThanImpl(Vector128.Create(a), (Vector128<int>)b);
        public static Vector2b operator <(int a, Vector2i b) => (Vector2b)VectorConstants.LessThanImpl(Vector128.Create(a), (Vector128<int>)b);

        #endregion

        #region Conversion Operators

        public static unsafe explicit operator Vector2i(Vector128<int> a) => *(Vector2i*)&a;
        public static unsafe explicit operator Vector2i(Vector128<long> a) => *(Vector2i*)&a;
        public static unsafe explicit operator Vector128<int>(Vector2i a) => Sse2.LoadVector128((int*)&a);

        #endregion
    }
}
