#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable ConvertToAutoProperty

// ReSharper disable InconsistentNaming

// ReSharper disable RedundantCast

#endregion

namespace Automata.Numerics
{
    public readonly partial struct Vector3i
    {
        public static Vector3i Zero { get; } = new Vector3i(0);
        public static Vector3i One { get; } = new Vector3i(1);

        private readonly int _X;
        private readonly int _Y;
        private readonly int _Z;
        private readonly int _W;

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

        #region Constructors

        public Vector3i(int xyz) => (_X, _Y, _Z, _W) = (xyz, xyz, xyz, 0);

        public Vector3i(int x, int y) => (_X, _Y, _Z, _W) = (x, y, 0, 0);

        public Vector3i(int x, int y, int z) => (_X, _Y, _Z, _W) = (x, y, z, 0);

        #endregion

        #region Mathematic Operators

        public static Vector3i operator &(Vector3i a, Vector3i b) => (Vector3i)BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3i operator |(Vector3i a, Vector3i b) => (Vector3i)BitwiseOrImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector3i operator +(Vector3i a, Vector3i b) => (Vector3i)AddImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3i operator +(Vector3i a, int b) => (Vector3i)AddImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector3i operator +(int a, Vector3i b) => (Vector3i)AddImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector3i operator -(Vector3i a, Vector3i b) => (Vector3i)SubtractImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3i operator -(Vector3i a, int b) => (Vector3i)SubtractImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector3i operator -(int a, Vector3i b) => (Vector3i)SubtractImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector3i operator *(Vector3i a, Vector3i b) => (Vector3i)MultiplyImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3i operator *(Vector3i a, int b) => (Vector3i)MultiplyImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector3i operator *(int a, Vector3i b) => (Vector3i)MultiplyImpl(Vector128.Create(a), (Vector128<int>)b);

        public static Vector3b operator >(Vector3i a, Vector3i b) => (Vector3b)GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b);
        public static Vector3b operator <(Vector3i a, Vector3i b) => (Vector3b)LessThanImpl((Vector128<int>)a, (Vector128<int>)b);

        public static Vector3b operator >(Vector3i a, int b) => (Vector3b)GreaterThanImpl((Vector128<int>)a, Vector128.Create(b));
        public static Vector3b operator <(Vector3i a, int b) => (Vector3b)LessThanImpl((Vector128<int>)a, Vector128.Create(b));

        public static Vector3b operator >(int a, Vector3i b) => (Vector3b)GreaterThanImpl(Vector128.Create(a), (Vector128<int>)b);
        public static Vector3b operator <(int a, Vector3i b) => (Vector3b)LessThanImpl(Vector128.Create(a), (Vector128<int>)b);

        #endregion

        #region Conversion Operators

        public static unsafe explicit operator Vector3i(Vector128<int> a) => *(Vector3i*)&a;
        public static unsafe explicit operator Vector3i(Vector128<long> a) => *(Vector3i*)&a;
        public static unsafe explicit operator Vector128<int>(Vector3i a) => Sse2.LoadVector128((int*)&a);

        #endregion
    }
}
