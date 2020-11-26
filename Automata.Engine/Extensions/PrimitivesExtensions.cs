using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;

namespace Automata.Engine.Extensions
{
    public static class PrimitivesExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AsByte(this bool a) => (byte)(Unsafe.As<bool, byte>(ref a) * byte.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AsBool(this byte a) => Unsafe.As<byte, bool>(ref a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FirstByte(this double a) => Unsafe.As<double, byte>(ref a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Unlerp(this float interpolant, float a, float b) => (interpolant - a) / (b - a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRadians(this float degrees) => degrees * ((float)Math.PI / 180f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CountBits(this uint i)
        {
            i -= (i >> 1) & 0x55555555;
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (byte)((((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Marshal(this string str, NativeStringEncoding nativeStringEncoding = NativeStringEncoding.LPStr) =>
            SilkMarshal.StringToPtr(str, nativeStringEncoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint GreatestCommonMultipleWith(this nint a, nint b)
        {
            while (b != 0)
            {
                nint temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint LeastCommonMultipleWith(this nint a, nint b) => (a / a.GreatestCommonMultipleWith(b)) * b;
    }
}
