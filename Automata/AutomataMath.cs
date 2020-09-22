#region

using System;
using System.Collections.Generic;
using System.Numerics;

#endregion

// ReSharper disable InconsistentNaming

namespace Automata
{
    public class AutomataMath
    {
        public static float UnLerp(float a, float b, float interpolant) => (interpolant - a) / (b - a);

        public static float ToRadians(float degrees) => degrees * ((float)Math.PI / 180f);
        public static Vector3 ToRadians(Vector3 degrees) => new Vector3(ToRadians(degrees.X), ToRadians(degrees.Y), ToRadians(degrees.Z));

        // row major
        public static unsafe Span<float> UnrollMatrix4x4RowMajor(Matrix4x4 matrix)
        {
            float* matrixUnrolled = stackalloc float[]
            {
                matrix.M11,
                matrix.M12,
                matrix.M13,
                matrix.M14,
                matrix.M21,
                matrix.M22,
                matrix.M23,
                matrix.M24,
                matrix.M31,
                matrix.M32,
                matrix.M33,
                matrix.M34,
                matrix.M41,
                matrix.M42,
                matrix.M43,
                matrix.M44,
            };

            return new Span<float>(matrixUnrolled, 16);
        }

        public static unsafe Span<float> UnrollMatrix4x4ColumnMajor(Matrix4x4 matrix)
        {
            float* matrixUnrolled = stackalloc float[]
            {
                matrix.M11,
                matrix.M21,
                matrix.M31,
                matrix.M41,
                matrix.M12,
                matrix.M22,
                matrix.M31,
                matrix.M42,
                matrix.M13,
                matrix.M23,
                matrix.M33,
                matrix.M43,
                matrix.M14,
                matrix.M24,
                matrix.M34,
                matrix.M44,
            };
            return new Span<float>(matrixUnrolled, 16);
        }

        public static IEnumerable<float> UnrollVector3(Vector3 vector3)
        {
            yield return vector3.X;
            yield return vector3.Y;
            yield return vector3.Z;
        }

        public static int Wrap(int v, int delta, int minVal, int maxVal)
        {
            int mod = (maxVal + 1) - minVal;
            v += delta - minVal;
            v += (1 - (v / mod)) * mod;
            return (v % mod) + minVal;
        }

        public static unsafe byte BoolToByte(bool a) => (byte)(*(byte*)&a * byte.MaxValue);
    }
}
