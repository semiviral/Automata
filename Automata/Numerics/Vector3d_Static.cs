#region

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Automata.Numerics
{
    public readonly partial struct Vector3d
    {
        #region Intrinsics

        private static Vector3b EqualsImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X == b0.X, a0.Y == b0.Y, a0.Z == b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X == b0, a0.Y == b0, a0.Z == b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 == b0.X, a0 == b0.Y, a0 == b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3b NotEqualsImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedNotEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X != b0.X, a0.Y != b0.Y, a0.Z != b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedNotEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X != b0, a0.Y != b0, a0.Z != b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedNotEqualSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 != b0.X, a0 != b0.Y, a0 != b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3d AddImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Add((Vector256<double>)a, (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X + b0.X, a0.Y + b0.Y, a0.Z + b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d AddImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Add((Vector256<double>)a, Vector256.Create(b));
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X + b0, a0.Y + b0, a0.Z + b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d AddImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Add(Vector256.Create(a), (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 + b0.X, a0 + b0.Y, a0 + b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3d SubtractImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Subtract((Vector256<double>)a, (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X - b0.X, a0.Y - b0.Y, a0.Z - b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d SubtractImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Subtract((Vector256<double>)a, Vector256.Create(b));
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X - b0, a0.Y - b0, a0.Z - b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d SubtractImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Subtract(Vector256.Create(a), (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 - b0.X, a0 - b0.Y, a0 - b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3d MultiplyImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Multiply((Vector256<double>)a, (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X * b0.X, a0.Y * b0.Y, a0.Z * b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d MultiplyImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Multiply((Vector256<double>)a, Vector256.Create(b));
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X * b0, a0.Y * b0, a0.Z * b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d MultiplyImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Multiply(Vector256.Create(a), (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 * b0.X, a0 * b0.Y, a0 * b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3d DivideImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Divide((Vector256<double>)a, (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X / b0.X, a0.Y / b0.Y, a0.Z / b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d DivideImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Divide((Vector256<double>)a, Vector256.Create(b));
            }
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X / b0, a0.Y / b0, a0.Z / b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d DivideImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3d)Avx.Divide(Vector256.Create(a), (Vector256<double>)b);
            }
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 / b0.X, a0 / b0.Y, a0 / b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3b GreaterThanImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedGreaterThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X > b0.X, a0.Y > b0.Y, a0.Z > b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedGreaterThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X > b0, a0.Y > b0, a0.Z > b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedGreaterThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 > b0.X, a0 > b0.Y, a0 > b0.Z);

                return SoftwareFallback(a, b);
            }
        }


        private static Vector3b LessThanImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedLessThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X < b0.X, a0.Y < b0.Y, a0.Z < b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedLessThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X < b0, a0.Y < b0, a0.Z < b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported)
            {
                return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedLessThanSignaling);
            }
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 < b0.X, a0 < b0.Y, a0 < b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion
    }
}
