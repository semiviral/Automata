#region

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming

namespace Automata.Engine
{
    public static class AutomataMath
    {
        public static float UnLerp(float a, float b, float interpolant) => (interpolant - a) / (b - a);

        public static float ToRadians(float degrees) => degrees * ((float)Math.PI / 180f);
        public static Vector3 ToRadians(Vector3 degrees) => new Vector3(ToRadians(degrees.X), ToRadians(degrees.Y), ToRadians(degrees.Z));

        // row major
        public static bool TryUnroll(this Matrix4x4 matrix, ref Span<float> unrolled)
        {
            Span<byte> span = MemoryMarshal.Cast<float, byte>(unrolled);
            return MemoryMarshal.TryWrite(span, ref matrix);
        }

        public static int Wrap(int v, int delta, int minVal, int maxVal)
        {
            int mod = (maxVal + 1) - minVal;
            v += delta - minVal;
            v += (1 - (v / mod)) * mod;
            return (v % mod) + minVal;
        }

        public static byte AsByte(this bool a) => (byte)(Unsafe.As<bool, byte>(ref a) * byte.MaxValue);
        public static bool AsBool(this byte a) => Unsafe.As<byte, bool>(ref a);
        public static byte FirstByte(this double a) => Unsafe.As<double, byte>(ref a);

        public static Vector3 RoundBy(this Vector3 a, Vector3 b) =>
            new Vector3((float)Math.Floor(a.X / b.X), (float)Math.Floor(a.Y / b.Y), (float)Math.Floor(a.Z / b.Z)) * b;
    }
}