#region

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion


namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector2i
    {
        public static Vector2i Project2D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            Math.DivRem(xQuotient, bounds, out int z);
            return new Vector2i(x, z);
        }

        public static int Project1D(Vector2i a, int size) => a.X + (size * a.Y);

        public static int Project1D(int a, int b, int size) => a + (size * b);

        public static int Sum(Vector2i a) => a.X + a.Y;


        #region Intrinsics

        #region EqualsImpl

        private static Vector2b EqualsImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareEqual((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2b(a0.X == b0.X, a0.Y == b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b EqualsImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareEqual((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, int b0) => new Vector2b(a0.X == b0, a0.Y == b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b EqualsImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareEqual(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2i b0) => new Vector2b(a0 == b0.X, a0 == b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseAndImpl

        private static Vector2i BitwiseAndImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.And((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X & b0.X, a0.Y & b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i BitwiseAndImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.And((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X & b0, a0.Y & b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i BitwiseAndImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.And(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 & b0.X, a0 & b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseOrImpl

        private static Vector2i BitwiseOrImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Or((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X | b0.X, a0.Y | b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i BitwiseOrImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Or((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X | b0, a0.Y | b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i BitwiseOrImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Or(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 | b0.X, a0 | b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region AddImpl

        private static Vector2i AddImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Add((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X + b0.X, a0.Y + b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i AddImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Add((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X + b0, a0.Y + b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i AddImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Add(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 + b0.X, a0 + b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region SubtractImpl

        private static Vector2i SubtractImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Subtract((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X - b0.X, a0.Y - b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i SubtractImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Subtract((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X - b0, a0.Y - b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i SubtractImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2i)Sse2.Subtract(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 - b0.X, a0 - b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region MultiplyImpl

        private static Vector2i MultiplyImpl(Vector2i a, Vector2i b)
        {
            if (Sse41.IsSupported)
            {
                return (Vector2i)Sse41.MultiplyLow((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X * b0.X, a0.Y * b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i MultiplyImpl(Vector2i a, int b)
        {
            if (Sse41.IsSupported)
            {
                return (Vector2i)Sse41.MultiplyLow((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X * b0, a0.Y * b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2i MultiplyImpl(int a, Vector2i b)
        {
            if (Sse41.IsSupported)
            {
                return (Vector2i)Sse41.MultiplyLow(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 * b0.X, a0 * b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region DivideImpl

        private static Vector2i DivideImpl(Vector2i a, Vector2i b)
        {
            static Vector2i SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2i(a0.X / b0.X, a0.Y / b0.Y);

            return SoftwareFallback(a, b);
        }

        private static Vector2i DivideImpl(Vector2i a, int b)
        {
            static Vector2i SoftwareFallback(Vector2i a0, int b0) => new Vector2i(a0.X / b0, a0.Y / b0);

            return SoftwareFallback(a, b);
        }

        private static Vector2i DivideImpl(int a, Vector2i b)
        {
            static Vector2i SoftwareFallback(int a0, Vector2i b0) => new Vector2i(a0 / b0.X, a0 / b0.Y);

            return SoftwareFallback(a, b);
        }

        #endregion


        private static Vector2b GreaterThanImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareGreaterThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2b(a0.X > b0.X, a0.Y > b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b GreaterThanImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareGreaterThan((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, int b0) => new Vector2b(a0.X > b0, a0.Y > b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b GreaterThanImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareGreaterThan(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2i b0) => new Vector2b(a0 > b0.X, a0 > b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(Vector2i a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareLessThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, Vector2i b0) => new Vector2b(a0.X < b0.X, a0.Y < b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(Vector2i a, int b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareLessThan((Vector128<int>)a, Vector128.Create(b));
            }
            else
            {
                static Vector2b SoftwareFallback(Vector2i a0, int b0) => new Vector2b(a0.X < b0, a0.Y < b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(int a, Vector2i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector2b)Sse2.CompareLessThan(Vector128.Create(a), (Vector128<int>)b);
            }
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2i b0) => new Vector2b(a0 < b0.X, a0 < b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion
    }
}
