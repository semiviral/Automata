using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Automata.Engine.Extensions;

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector4<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Abs(Vector4<T> a) => System.Numerics.Vector.Abs(a.AsVector()).AsVector4();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Floor(Vector4<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return System.Numerics.Vector.Floor(a.AsVector<T, float>()).AsVector4<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return System.Numerics.Vector.Floor(a.AsVector<T, double>()).AsVector4<double, T>();
            }
            else
            {
                return a;
            }
        }
    }
}
