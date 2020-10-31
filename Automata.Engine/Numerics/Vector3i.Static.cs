#region

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

#endregion


namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector3i
    {
        public static Vector3i FromVector3(Vector3 a) => new Vector3i((int)a.X, (int)a.Y, (int)a.Z);
        public static Vector3i FromVector3d(Vector3d a) => new Vector3i((int)a.X, (int)a.Y, (int)a.Z);

        public static Vector3i RoundBy(Vector3i a, Vector3i by) => (a / by) * by;
        public static Vector3i RoundBy(Vector3i a, int by) => (a / by) * by;

        public static Vector3i Project3D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            int zQuotient = Math.DivRem(xQuotient, bounds, out int z);
            int y = zQuotient % bounds;
            return new Vector3i(x, y, z);
        }

        public static int Project1D(Vector3i a, int size) => a.X + (size * (a.Z + (size * a.Y)));
        public static int Project1D(int x, int y, int z, int size) => x + (size * (z + (size * y)));

        public static long Sum(Vector3i a) => a.X + a.Y + a.Z;

        public static Vector3i Abs(Vector3i a) => AbsImpl(a);

        public static Vector3i Mod(Vector3i a, Vector3i mod) => ModImpl(a, mod);
        public static Vector3i Mod(Vector3i a, int mod) => ModImpl(a, mod);
        public static Vector3i Mod(int a, Vector3i mod) => ModImpl(a, mod);


        #region Intrinsics

        #region EqualsImpl

        private static Vector3b EqualsImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareEqual((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, Vector3i b) => new Vector3b(a.X == b.X, a.Y == b.Y, a.Z == b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareEqual((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, int b) => new Vector3b(a.X == b, a.Y == b, a.Z == b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareEqual(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(int a, Vector3i b) => new Vector3b(a == b.X, a == b.Y, a == b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseAndImpl

        private static Vector3i BitwiseAndImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.And((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X & b.X, a.Y & b.Y, a.Z & b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i BitwiseAndImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.And((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X & b, a.Y & b, a.Z & b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i BitwiseAndImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.And(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a & b.X, a & b.Y, a & b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseOrImpl

        private static Vector3i BitwiseOrImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Or((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X | b.X, a.Y | b.Y, a.Z | b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i BitwiseOrImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Or((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X | b, a.Y | b, a.Z | b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i BitwiseOrImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Or(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a | b.X, a | b.Y, a | b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region AddImpl

        private static Vector3i AddImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Add((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i AddImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Add((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X + b, a.Y + b, a.Z + b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i AddImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Add(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a + b.X, a + b.Y, a + b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region SubtractImpl

        private static Vector3i SubtractImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Subtract((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i SubtractImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Subtract((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X - b, a.Y - b, a.Z - b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i SubtractImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Subtract(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a - b.X, a - b.Y, a - b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region MultiplyImpl

        private static Vector3i MultiplyImpl(Vector3i a, Vector3i b)
        {
            static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

            if (Sse41.IsSupported)
            {
                return (Vector3i)Sse41.MultiplyLow((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i MultiplyImpl(Vector3i a, int b)
        {
            static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X * b, a.Y * b, a.Z * b);

            if (Sse41.IsSupported)
            {
                return (Vector3i)Sse41.MultiplyLow((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                return SoftwareFallback(a, b);
            }
        }

        private static Vector3i MultiplyImpl(int a, Vector3i b)
        {
            static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a * b.X, a * b.Y, a * b.Z);

            if (Sse41.IsSupported)
            {
                return (Vector3i)Sse41.MultiplyLow(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region DivideImpl

        private static Vector3i DivideImpl(Vector3i a, Vector3i b)
        {
            static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

            return SoftwareFallback(a, b);
        }

        private static Vector3i DivideImpl(Vector3i a, int b)
        {
            static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X / b, a.Y / b, a.Z / b);

            return SoftwareFallback(a, b);
        }

        private static Vector3i DivideImpl(int a, Vector3i b)
        {
            static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a / b.X, a / b.Y, a / b.Z);

            return SoftwareFallback(a, b);
        }

        #endregion


        #region GreaterThanImpl

        private static Vector3b GreaterThanImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareGreaterThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, Vector3i b) => new Vector3b(a.X > b.X, a.Y > b.Y, a.Z > b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareGreaterThan((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, int b) => new Vector3b(a.X > b, a.Y > b, a.Z > b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareGreaterThan(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(int a, Vector3i b) => new Vector3b(a > b.X, a > b.Y, a > b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region LessThanImpl

        private static Vector3b LessThanImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareLessThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, Vector3i b) => new Vector3b(a.X < b.X, a.Y < b.Y, a.Z < b.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(Vector3i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareLessThan((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a, int b) => new Vector3b(a.X < b, a.Y < b, a.Z < b);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(int a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareLessThan(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(int a, Vector3i b) => new Vector3b(a < b.X, a < b.Y, a < b.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region AbsImpl

        private static Vector3i AbsImpl(Vector3i a)
        {
            static Vector3i SoftwareFallback(Vector3i a) => new Vector3i(Math.Abs(a.X), Math.Abs(a.Y), Math.Abs(a.Z));

            if (Ssse3.IsSupported)
            {
                return (Vector3i)Ssse3.Abs((Vector128<int>)a);
            }
            else
            {
                return SoftwareFallback(a);
            }
        }

        #endregion


        #region RemainderImpl

        private static Vector3i RemainderImpl(Vector3i a, Vector3i b)
        {
            static Vector3i SoftwareFallback(Vector3i a, Vector3i b) => new Vector3i(a.X % b.X, a.Y % b.Y, a.Z % b.Z);

            return SoftwareFallback(a, b);
        }

        private static Vector3i RemainderImpl(Vector3i a, int b)
        {
            static Vector3i SoftwareFallback(Vector3i a, int b) => new Vector3i(a.X % b, a.Y % b, a.Z % b);

            return SoftwareFallback(a, b);
        }

        private static Vector3i RemainderImpl(int a, Vector3i b)
        {
            static Vector3i SoftwareFallback(int a, Vector3i b) => new Vector3i(a % b.X, a % b.Y, a % b.Z);

            return SoftwareFallback(a, b);
        }

        #endregion


        #region ModImpl

        private static Vector3i ModImpl(Vector3i a, Vector3i mod)
        {
            static Vector3i SoftwareFallback(Vector3i a, Vector3i mod) => (Abs(a * mod) + a) % mod;

            return SoftwareFallback(a, mod);
        }

        private static Vector3i ModImpl(Vector3i a, int mod)
        {
            static Vector3i SoftwareFallback(Vector3i a, int mod) => (Abs(a * mod) + a) % mod;

            return SoftwareFallback(a, mod);
        }

        private static Vector3i ModImpl(int a, Vector3i mod)
        {
            static Vector3i SoftwareFallback(int a, Vector3i mod) => (Abs(a * mod) + a) % mod;

            return SoftwareFallback(a, mod);
        }

        #endregion

        #endregion
    }
}
