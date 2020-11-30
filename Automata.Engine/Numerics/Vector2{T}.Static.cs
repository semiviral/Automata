using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Automata.Engine.Extensions;

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector2<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Abs(Vector2<T> a)
        {
            if ((typeof(T) == typeof(int)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128<T, int>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128<T, short>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128<T, sbyte>()).AsVector2<byte, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                return new Vector2<T>(
                    MathF.Abs(a.X.Coerce<T, float>()).Coerce<float, T>(),
                    MathF.Abs(a.Y.Coerce<T, float>()).Coerce<float, T>());
            }
            else if (typeof(T) == typeof(double))
            {
                return new Vector2<T>(
                    Math.Abs(a.X.Coerce<T, double>()).Coerce<double, T>(),
                    Math.Abs(a.Y.Coerce<T, double>()).Coerce<double, T>());
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Floor(Vector2<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Floor(a.AsVector128<T, float>()).AsVector2<float, T>()
                    : System.Numerics.Vector.Floor(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Avx.IsSupported
                    ? Avx.Floor(a.AsVector256<T, float>()).AsVector2<float, T>()
                    : System.Numerics.Vector.Floor(a.AsVector<T, double>()).AsVector2<double, T>();
            }
            else
            {
                return a;
            }
        }
    }
}
