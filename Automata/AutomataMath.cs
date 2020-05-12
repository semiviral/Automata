#region

using System;
using System.Numerics;

#endregion

// ReSharper disable InconsistentNaming

namespace Automata
{
    public class AutomataMath
    {
        public static float UnLerp(float a, float b, float interpolant) => (interpolant - a) / (b - a);

        public static float ToRadians(float degrees) => degrees * ((float)Math.PI / 180f);

        public static float[] UnrollMatrix4x4(Matrix4x4 matrix) =>
            new[]
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
                matrix.M44
            };

        public static int Wrap(int v, int delta, int minVal, int maxVal)
        {
            int mod = (maxVal + 1) - minVal;
            v += delta - minVal;
            v += (1 - (v / mod)) * mod;
            return (v % mod) + minVal;
        }
    }
}
