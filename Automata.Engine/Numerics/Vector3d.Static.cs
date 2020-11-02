#region

using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion


// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector3d
    {
        public static Vector3 AsVector3(Vector3d a) => new Vector3((float)a.X, (float)a.Y, (float)a.Z);

        public static Vector3d Transform(Vector3d value, Quaternion rotation)
        {
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;

            double wx2 = rotation.W * x2;
            double wy2 = rotation.W * y2;
            double wz2 = rotation.W * z2;
            double xx2 = rotation.X * x2;
            double xy2 = rotation.X * y2;
            double xz2 = rotation.X * z2;
            double yy2 = rotation.Y * y2;
            double yz2 = rotation.Y * z2;
            double zz2 = rotation.Z * z2;

            return new Vector3d(
                (value.X * (1.0d - yy2 - zz2)) + (value.Y * (xy2 - wz2)) + (value.Z * (xz2 + wy2)),
                (value.X * (xy2 + wz2)) + (value.Y * (1.0d - xx2 - zz2)) + (value.Z * (yz2 - wx2)),
                (value.X * (xz2 - wy2)) + (value.Y * (yz2 + wx2)) + (value.Z * (1.0d - xx2 - yy2)));
        }


        #region Intrinsics

        private static Vector3b EqualsImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X == b0.X, a0.Y == b0.Y, a0.Z == b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X == b0, a0.Y == b0, a0.Z == b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b EqualsImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 == b0.X, a0 == b0.Y, a0 == b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedNotEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X != b0.X, a0.Y != b0.Y, a0.Z != b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedNotEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X != b0, a0.Y != b0, a0.Z != b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedNotEqualSignaling);
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 != b0.X, a0 != b0.Y, a0 != b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d AddImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Add((Vector256<double>)a, (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X + b0.X, a0.Y + b0.Y, a0.Z + b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d AddImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Add((Vector256<double>)a, Vector256.Create(b));
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X + b0, a0.Y + b0, a0.Z + b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d AddImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Add(Vector256.Create(a), (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 + b0.X, a0 + b0.Y, a0 + b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d SubtractImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Subtract((Vector256<double>)a, (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X - b0.X, a0.Y - b0.Y, a0.Z - b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d SubtractImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Subtract((Vector256<double>)a, Vector256.Create(b));
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X - b0, a0.Y - b0, a0.Z - b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d SubtractImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Subtract(Vector256.Create(a), (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 - b0.X, a0 - b0.Y, a0 - b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d MultiplyImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Multiply((Vector256<double>)a, (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X * b0.X, a0.Y * b0.Y, a0.Z * b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d MultiplyImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Multiply((Vector256<double>)a, Vector256.Create(b));
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X * b0, a0.Y * b0, a0.Z * b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d MultiplyImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Multiply(Vector256.Create(a), (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 * b0.X, a0 * b0.Y, a0 * b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d DivideImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Divide((Vector256<double>)a, (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3d(a0.X / b0.X, a0.Y / b0.Y, a0.Z / b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d DivideImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Divide((Vector256<double>)a, Vector256.Create(b));
            else
            {
                static Vector3d SoftwareFallback(Vector3d a0, double b0) => new Vector3d(a0.X / b0, a0.Y / b0, a0.Z / b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3d DivideImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3d)Avx.Divide(Vector256.Create(a), (Vector256<double>)b);
            else
            {
                static Vector3d SoftwareFallback(double a0, Vector3d b0) => new Vector3d(a0 / b0.X, a0 / b0.Y, a0 / b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedGreaterThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X > b0.X, a0.Y > b0.Y, a0.Z > b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedGreaterThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X > b0, a0.Y > b0, a0.Z > b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b GreaterThanImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedGreaterThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 > b0.X, a0 > b0.Y, a0 > b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(Vector3d a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, (Vector256<double>)b, FloatComparisonMode.OrderedLessThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, Vector3d b0) => new Vector3b(a0.X < b0.X, a0.Y < b0.Y, a0.Z < b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(Vector3d a, double b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare((Vector256<double>)a, Vector256.Create(b), FloatComparisonMode.OrderedLessThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(Vector3d a0, double b0) => new Vector3b(a0.X < b0, a0.Y < b0, a0.Z < b0);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b LessThanImpl(double a, Vector3d b)
        {
            if (Avx.IsSupported) return (Vector3b)Avx.Compare(Vector256.Create(a), (Vector256<double>)b, FloatComparisonMode.OrderedLessThanSignaling);
            else
            {
                static Vector3b SoftwareFallback(double a0, Vector3d b0) => new Vector3b(a0 < b0.X, a0 < b0.Y, a0 < b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        #endregion
    }
}
