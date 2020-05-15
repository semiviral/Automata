#region

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

#endregion


namespace Automata.Numerics
{
    public readonly partial struct Vector3i
    {
        public static Vector3i FromVector3(Vector3 a) => new Vector3i((int)a.X, (int)a.Y, (int)a.Z);

        public static Vector3i Project3D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            int zQuotient = Math.DivRem(xQuotient, bounds, out int z);
            int y = zQuotient % bounds;
            return new Vector3i(x, y, z);
        }

        public static int Project1D(Vector3i a, int size) => a.X + (size * (a.Z + (size * a.Y)));

        public static int Sum(Vector3i a) => a.X + a.Y + a.Z;


        #region Intrinsics

        private static Vector3b EqualsImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareEqual((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3b(a0.X == b0.X, a0.Y == b0.Y, a0.Z == b0.Z);

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
                static Vector3b SoftwareFallback(Vector3i a0, int b0) => new Vector3b(a0.X == b0, a0.Y == b0, a0.Z == b0);

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
                static Vector3b SoftwareFallback(int a0, Vector3i b0) => new Vector3b(a0 == b0.X, a0 == b0.Y, a0 == b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3b NotEqualsImpl(Vector3i a, Vector3i b)
        {
            static Vector3b SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3b(a0.X != b0.X, a0.Y != b0.Y, a0.Z != b0.Z);

            return SoftwareFallback(a, b);
        }

        private static Vector3b NotEqualsImpl(Vector3i a, int b)
        {
            static Vector3b SoftwareFallback(Vector3i a0, int b0) => new Vector3b(a0.X != b0, a0.Y != b0, a0.Z != b0);

            return SoftwareFallback(a, b);
        }

        private static Vector3b NotEqualsImpl(int a, Vector3i b)
        {
            static Vector3b SoftwareFallback(int a0, Vector3i b0) => new Vector3b(a0 != b0.X, a0 != b0.Y, a0 != b0.Z);

            return SoftwareFallback(a, b);
        }


        private static Vector3i BitwiseAndImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.And((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3i(a0.X & b0.X, a0.Y & b0.Y, a0.Z & b0.Z);

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
                static Vector3i SoftwareFallback(Vector3i a0, int b0) => new Vector3i(a0.X & b0, a0.Y & b0, a0.Z & b0);

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
                static Vector3i SoftwareFallback(int a0, Vector3i b0) => new Vector3i(a0 & b0.X, a0 & b0.Y, a0 & b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3i BitwiseOrImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Or((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3i(a0.X | b0.X, a0.Y | b0.Y, a0.Z | b0.Z);

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
                static Vector3i SoftwareFallback(Vector3i a0, int b0) => new Vector3i(a0.X | b0, a0.Y | b0, a0.Z | b0);

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
                static Vector3i SoftwareFallback(int a0, Vector3i b0) => new Vector3i(a0 | b0.X, a0 | b0.Y, a0 | b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3i AddImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Add((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3i(a0.X + b0.X, a0.Y + b0.Y, a0.Z + b0.Z);

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
                static Vector3i SoftwareFallback(Vector3i a0, int b0) => new Vector3i(a0.X + b0, a0.Y + b0, a0.Z + b0);

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
                static Vector3i SoftwareFallback(int a0, Vector3i b0) => new Vector3i(a0 + b0.X, a0 + b0.Y, a0 + b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3i SubtractImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3i)Sse2.Subtract((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3i SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3i(a0.X - b0.X, a0.Y - b0.Y, a0.Z - b0.Z);

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
                static Vector3i SoftwareFallback(Vector3i a0, int b0) => new Vector3i(a0.X - b0, a0.Y - b0, a0.Z - b0);

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
                static Vector3i SoftwareFallback(int a0, Vector3i b0) => new Vector3i(a0 - b0.X, a0 - b0.Y, a0 - b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3i MultiplyImpl(Vector3i a, Vector3i b)
        {
            static Vector3i SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3i(a0.X * b0.X, a0.Y * b0.Y, a0.Z * b0.Z);

            return SoftwareFallback(a, b);
        }

        private static Vector3i MultiplyImpl(Vector3i a, int b)
        {
            static Vector3i SoftwareFallback(Vector3i a0, int b0) => new Vector3i(a0.X * b0, a0.Y * b0, a0.Z * b0);

            return SoftwareFallback(a, b);
        }

        private static Vector3i MultiplyImpl(int a, Vector3i b)
        {
            static Vector3i SoftwareFallback(int a0, Vector3i b0) => new Vector3i(a0 * b0.X, a0 * b0.Y, a0 * b0.Z);

            return SoftwareFallback(a, b);
        }


        private static Vector3b GreaterThanImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareGreaterThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3b(a0.X > b0.X, a0.Y > b0.Y, a0.Z > b0.Z);

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
                static Vector3b SoftwareFallback(Vector3i a0, int b0) => new Vector3b(a0.X > b0, a0.Y > b0, a0.Z > b0);

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
                static Vector3b SoftwareFallback(int a0, Vector3i b0) => new Vector3b(a0 > b0.X, a0 > b0.Y, a0 > b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3b LessThanImpl(Vector3i a, Vector3i b)
        {
            if (Sse2.IsSupported)
            {
                return (Vector3b)Sse2.CompareLessThan((Vector128<int>)a, (Vector128<int>)b);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3i a0, Vector3i b0) => new Vector3b(a0.X < b0.X, a0.Y < b0.Y, a0.Z < b0.Z);

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
                static Vector3b SoftwareFallback(Vector3i a0, int b0) => new Vector3b(a0.X < b0, a0.Y < b0, a0.Z < b0);

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
                static Vector3b SoftwareFallback(int a0, Vector3i b0) => new Vector3b(a0 < b0.X, a0 < b0.Y, a0 < b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion
    }
}
