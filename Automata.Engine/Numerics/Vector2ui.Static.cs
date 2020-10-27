#region

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion


namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector2ui
    {
        public static int Sum(Vector2ui a) => a.X + a.Y;


        #region Intrinsics

        #region EqualsImpl

        private static Vector2b EqualsImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareEqual((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2b(a0.X == b0.X, a0.Y == b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b EqualsImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareEqual((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, int b0) => new Vector2b(a0.X == b0, a0.Y == b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b EqualsImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareEqual(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2ui b0) => new Vector2b(a0 == b0.X, a0 == b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseAndImpl

        private static Vector2ui BitwiseAndImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.And((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X & b0.X, a0.Y & b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui BitwiseAndImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.And((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X & b0, a0.Y & b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui BitwiseAndImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.And(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 & b0.X, a0 & b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region BitwiseOrImpl

        private static Vector2ui BitwiseOrImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Or((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X | b0.X, a0.Y | b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui BitwiseOrImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Or((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X | b0, a0.Y | b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui BitwiseOrImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Or(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 | b0.X, a0 | b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region AddImpl

        private static Vector2ui AddImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Add((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X + b0.X, a0.Y + b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui AddImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Add((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X + b0, a0.Y + b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui AddImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Add(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 + b0.X, a0 + b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region SubtractImpl

        private static Vector2ui SubtractImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Subtract((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X - b0.X, a0.Y - b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui SubtractImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Subtract((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X - b0, a0.Y - b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui SubtractImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2ui)Sse2.Subtract(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 - b0.X, a0 - b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region MultiplyImpl

        private static Vector2ui MultiplyImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse41.IsSupported) return (Vector2ui)Sse41.MultiplyLow((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X * b0.X, a0.Y * b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui MultiplyImpl(Vector2ui a, int b)
        {
            if (Sse41.IsSupported) return (Vector2ui)Sse41.MultiplyLow((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X * b0, a0.Y * b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2ui MultiplyImpl(int a, Vector2ui b)
        {
            if (Sse41.IsSupported) return (Vector2ui)Sse41.MultiplyLow(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 * b0.X, a0 * b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion


        #region DivideImpl

        private static Vector2ui DivideImpl(Vector2ui a, Vector2ui b)
        {
            static Vector2ui SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2ui(a0.X / b0.X, a0.Y / b0.Y);

            return SoftwareFallback(a, b);
        }

        private static Vector2ui DivideImpl(Vector2ui a, int b)
        {
            static Vector2ui SoftwareFallback(Vector2ui a0, int b0) => new Vector2ui(a0.X / b0, a0.Y / b0);

            return SoftwareFallback(a, b);
        }

        private static Vector2ui DivideImpl(int a, Vector2ui b)
        {
            static Vector2ui SoftwareFallback(int a0, Vector2ui b0) => new Vector2ui(a0 / b0.X, a0 / b0.Y);

            return SoftwareFallback(a, b);
        }

        #endregion


        private static Vector2b GreaterThanImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareGreaterThan((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2b(a0.X > b0.X, a0.Y > b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b GreaterThanImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareGreaterThan((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, int b0) => new Vector2b(a0.X > b0, a0.Y > b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b GreaterThanImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareGreaterThan(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2ui b0) => new Vector2b(a0 > b0.X, a0 > b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(Vector2ui a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareLessThan((Vector128<int>)a, (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, Vector2ui b0) => new Vector2b(a0.X < b0.X, a0.Y < b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(Vector2ui a, int b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareLessThan((Vector128<int>)a, Vector128.Create(b));
            else
            {
                static Vector2b SoftwareFallback(Vector2ui a0, int b0) => new Vector2b(a0.X < b0, a0.Y < b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b LessThanImpl(int a, Vector2ui b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareLessThan(Vector128.Create(a), (Vector128<int>)b);
            else
            {
                static Vector2b SoftwareFallback(int a0, Vector2ui b0) => new Vector2b(a0 < b0.X, a0 < b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        #endregion
    }
}
