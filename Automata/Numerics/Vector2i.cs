#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable RedundantCast

namespace Automata.Numerics
{
    public readonly partial struct Vector2i
    {
        public static Vector2i Zero { get; } = new Vector2i(0);
        public static Vector2i One { get; } = new Vector2i(1);

        private readonly int _W;

        public int X { get; }

        public int Y { get; }

        public int Z { get; }

        public int this[int index] => index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        #region Constructors

        public Vector2i(int xyz) => (X, Y, Z, _W) = (xyz, xyz, xyz, 0);

        public Vector2i(int x, int y) => (X, Y, Z, _W) = (x, y, 0, 0);

        public Vector2i(int x, int y, int z) => (X, Y, Z, _W) = (x, y, z, 0);

        #endregion

        #region Mathematic Operators

        public static Vector2i operator &(Vector2i a, Vector2i b) => (Vector2i)BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator |(Vector2i a, Vector2i b) => (Vector2i)BitwiseOrImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector2i operator +(Vector2i a, Vector2i b) => (Vector2i)AddImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator +(Vector2i a, int b) => (Vector2i)AddImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator +(int a, Vector2i b) => (Vector2i)AddImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2i operator -(Vector2i a, Vector2i b) => (Vector2i)SubtractImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator -(Vector2i a, int b) => (Vector2i)SubtractImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator -(int a, Vector2i b) => (Vector2i)SubtractImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2i operator *(Vector2i a, Vector2i b) => (Vector2i)MultiplyImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2i operator *(Vector2i a, int b) => (Vector2i)MultiplyImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2i operator *(int a, Vector2i b) => (Vector2i)MultiplyImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector2b operator >(Vector2i a, Vector2i b) => (Vector2b)GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector2b operator <(Vector2i a, Vector2i b) => (Vector2b)LessThanImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector2b operator >(Vector2i a, int b) => (Vector2b)GreaterThanImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector2b operator <(Vector2i a, int b) => (Vector2b)LessThanImpl((Vector128<int>)a, Vector128.Create(b));

        public static Vector2b operator >(int a, Vector2i b) => (Vector2b)GreaterThanImpl(Vector128.Create(a), (Vector128<int>)b);
        public static Vector2b operator <(int a, Vector2i b) => (Vector2b)LessThanImpl(Vector128.Create(a), (Vector128<int>)b);

        #endregion

        #region Conversion Operators

        public static unsafe explicit operator Vector2i(Vector128<int> a) => *(Vector2i*)&a;
        public static unsafe explicit operator Vector2i(Vector128<long> a) => *(Vector2i*)&a;
        public static unsafe explicit operator Vector128<int>(Vector2i a) => Sse2.LoadVector128((int*)&a);

        #endregion
    }
}
