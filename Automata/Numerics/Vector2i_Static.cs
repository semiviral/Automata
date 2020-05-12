#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable InconsistentNaming

// ReSharper disable RedundantCast

namespace Automata.Numerics
{
    public readonly partial struct Vector2i
    {
        public static Vector2i Select(Vector2i a, Vector2b selector) => (Vector2i)BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)selector);

        public static Vector2i SelectByGreaterThan(Vector2i select, Vector2i a, Vector2i b) =>
            (Vector2i)BitwiseAndImpl((Vector128<int>)select, GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static Vector2i SelectByLessThan(Vector2i select, Vector2i a, Vector2i b) =>
            (Vector2i)BitwiseAndImpl((Vector128<int>)select, LessThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static int Sum(Vector2i a) => a.X + a.Y + a.Z;

        public static Vector2i Project3D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            int zQuotient = Math.DivRem(xQuotient, bounds, out int z);
            int y = zQuotient % bounds;
            return new Vector2i(x, y, z);
        }

        public static int Project1D(Vector2i a, int size) => (int)(a.X + (size * (a.Z + (size * a.Y))));

        #region Impl

        private static Vector128<int> BitwiseAndImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.And(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> BitwiseOrImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Or(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> AddImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Add(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> SubtractImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Subtract(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> MultiplyImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse41.IsSupported)
            {
                // todo this conversion is slow
                return (Vector128<int>)(Vector2i)Sse41.Multiply(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> GreaterThanImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        private static Vector128<int> LessThanImpl(Vector128<int> a, Vector128<int> b)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a, b);
            }
            else
            {
                throw new NotSupportedException(nameof(Sse2));
            }
        }

        #endregion
    }
}
