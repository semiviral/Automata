using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Automata.Engine.Numerics.Shapes;

namespace Automata.Engine.Numerics
{
    public static class Vector
    {
        public static void ThrowNotSupportedGenericType() => throw new NotSupportedException("Generic vectors only support primitive types.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> Coerce<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            (Vector2<TTo>)(object)vector;

#region AsVector128

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> AsVector128<T>(this Vector2<T> vector)
            where T : unmanaged =>
            Unsafe.As<Vector2<T>, Vector128<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<TTo> AsVector128<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector2<TFrom>, Vector128<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<TTo> AsVector128<TFrom, TTo>(this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector3<TFrom>, Vector128<TTo>>(ref vector);

#endregion

#region AsVector2/3/4

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> AsVector2<T>(this Vector128<T> vector)
            where T : unmanaged =>
            Unsafe.As<Vector128<T>, Vector2<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> AsVector2<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector128<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<TTo> AsVector3<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector128<TFrom>, Vector3<TTo>>(ref vector);
    }

    #endregion
}
