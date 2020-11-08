#region

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion


// ReSharper disable InconsistentNaming

namespace Automata.Engine
{
    public static class AutomataMath
    {
        public static float Unlerp(float a, float b, float interpolant) => (interpolant - a) / (b - a);
        public static float ToRadians(float degrees) => degrees * ((float)Math.PI / 180f);

        // row major
        public static unsafe Span<float> Unroll(this Matrix4x4 matrix) => MemoryMarshal.CreateSpan(ref matrix.M11, sizeof(Matrix4x4) / sizeof(float));

        public static IEnumerable<float> UnrollColumnMajor(this Matrix4x4 matrix)
        {
            yield return matrix.M11;
            yield return matrix.M21;
            yield return matrix.M31;
            yield return matrix.M41;

            yield return matrix.M12;
            yield return matrix.M22;
            yield return matrix.M32;
            yield return matrix.M42;

            yield return matrix.M13;
            yield return matrix.M23;
            yield return matrix.M33;
            yield return matrix.M43;

            yield return matrix.M14;
            yield return matrix.M24;
            yield return matrix.M34;
            yield return matrix.M44;
        }

        public static byte AsByte(this bool a) => (byte)(Unsafe.As<bool, byte>(ref a) * byte.MaxValue);
        public static bool AsBool(this byte a) => Unsafe.As<byte, bool>(ref a);
        public static byte FirstByte(this double a) => Unsafe.As<double, byte>(ref a);

        public static Vector3 RoundBy(this Vector3 a, Vector3 by)
        {
            Vector3 rounded = a / by;
            rounded = new Vector3(MathF.Floor(rounded.X), MathF.Floor(rounded.Y), MathF.Floor(rounded.Z));
            return rounded * by;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CountBits(uint i)
        {
            i -= (i >> 1) & 0x55555555;
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (byte)((((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }
    }
}
