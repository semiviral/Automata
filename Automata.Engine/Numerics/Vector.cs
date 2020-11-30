using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public static class Vector
    {
        internal static void ThrowNotSupportedType() => throw new NotSupportedException("Generic vectors only support primitive types.");


        #region Vector<bool> Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All(Vector2<bool> a) => a.X && a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All(Vector3<bool> a) => a.X && a.Y && a.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All(Vector4<bool> a) => a.X && a.Y && a.Z && a.W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(Vector2<bool> a) => a.X || a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(Vector3<bool> a) => a.X || a.Y || a.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(Vector4<bool> a) => a.X || a.Y || a.Z || a.W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(Vector2<bool> a) => !a.X && !a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(Vector3<bool> a) => !a.X && !a.Y && !a.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(Vector4<bool> a) => !a.X && !a.Y && !a.Z && !a.W;

        #endregion


        #region Conversions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> Coerce<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            typeof(TFrom) == typeof(TTo)
                ? (Vector2<TTo>)(object)vector
                : new Vector2<TTo>((TTo)(object)vector.X, (TTo)(object)vector.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<TTo> Coerce<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            (Vector128<TTo>)(object)vector;


        #region Arbitrary

        public static Point AsPoint(this Vector2<int> vector) => Unsafe.As<Vector2<int>, Point>(ref vector);
        public static Vector2<int> AsVector(this Point point) => Unsafe.As<Point, Vector2<int>>(ref point);

        public static PointF AsPointF(this Vector2<float> vector) => Unsafe.As<Vector2<float>, PointF>(ref vector);
        public static Vector2<float> AsVector(this PointF pointF) => Unsafe.As<PointF, Vector2<float>>(ref pointF);

        public static Size AsSize(this Vector2<int> vector) => Unsafe.As<Vector2<int>, Size>(ref vector);
        public static Vector2<int> AsVector(this Size size) => Unsafe.As<Size, Vector2<int>>(ref size);

        #endregion


        #region Vector2/3/4 As

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> Reinterpret<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector2<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<TTo> Reinterpret<TFrom, TTo>(this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector3<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> Reinterpret<TFrom, TTo>(this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector4<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector2<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector3<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector4<TFrom>, TTo>(ref vector);

        #endregion


        #region AsVector

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> AsVector<T>(this Vector2<T> vector) where T : unmanaged => Unsafe.As<Vector2<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> AsVector<T>(this Vector3<T> vector) where T : unmanaged => Unsafe.As<Vector3<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<T> AsVector<T>(this Vector4<T> vector) where T : unmanaged => Unsafe.As<Vector4<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<TTo> AsVector<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged
            => Unsafe.As<Vector128<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<TTo> AsVector<TFrom, TTo>(this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged
            => Unsafe.As<Vector256<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<TTo> AsVector<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector2<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<TTo> AsVector<TFrom, TTo>(this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector3<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<TTo> AsVector<TFrom, TTo>(this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector4<TFrom>, Vector<TTo>>(ref vector);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector128<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> AsVector2<TFrom, TTo>(this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector256<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<TTo> AsVector3<TFrom, TTo>(this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector256<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector256<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> AsVector2<T>(this Vector<T> vector) where T : unmanaged => Unsafe.As<Vector<T>, Vector2<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> AsVector3<T>(this Vector<T> vector) where T : unmanaged => Unsafe.As<Vector<T>, Vector3<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AsVector4<T>(this Vector<T> vector) where T : unmanaged => Unsafe.As<Vector<T>, Vector4<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<TTo> AsVector2<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<TTo> AsVector3<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector4<TTo>>(ref vector);

        #endregion


        #region AsVector128

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> AsVector128<T>(this Vector2<T> vector)
            where T : unmanaged =>
            Unsafe.As<Vector2<T>, Vector128<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<TTo> AsVector128<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector128<TTo>>(ref vector);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<TTo> AsVector128<TFrom, TTo>(this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector4<TFrom>, Vector128<TTo>>(ref vector);

        #endregion


        #region AsVector256

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<TTo> AsVector256<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector256<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<TTo> AsVector256<TFrom, TTo>(this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector2<TFrom>, Vector256<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<TTo> AsVector256<TFrom, TTo>(this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector3<TFrom>, Vector256<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<TTo> AsVector256<TFrom, TTo>(this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector4<TFrom>, Vector256<TTo>>(ref vector);

        #endregion


        #region AsIntrinsic

        /// <summary>
        ///     Converts a given generic vector to its intrinsic variant.
        /// </summary>
        /// <remarks>
        ///     It's assumed that T is a valid type. No type checking is done by this method for performance.
        /// </remarks>
        /// <param name="vector">Vector to convert.</param>
        /// <returns>Intrinsic variant of the given vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 AsIntrinsic(this Vector2<float> vector) => Unsafe.As<Vector2<float>, Vector2>(ref vector);

        /// <inheritdoc cref="AsIntrinsic(Automata.Engine.Numerics.Vector2{float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AsIntrinsic(this Vector3<float> vector) => Unsafe.As<Vector3<float>, Vector3>(ref vector);

        /// <inheritdoc cref="AsIntrinsic(Automata.Engine.Numerics.Vector2{float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 AsIntrinsic(this Vector4<float> vector) => Unsafe.As<Vector4<float>, Vector4>(ref vector);

        #endregion


        #region AsGeneric

        /// <summary>
        ///     Converts a given intrinsic vector to its generic variant.
        /// </summary>
        /// <remarks>
        ///     It's assumed that T is a valid type. No type checking is done by this method for performance.
        /// </remarks>
        /// <param name="vector">Vector to convert.</param>
        /// <typeparam name="T">Unmanaged type to convert generic to.</typeparam>
        /// <returns>Generic variant of the given vector and type <typeparamref name="T" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> AsGeneric<T>(this Vector2 vector) where T : unmanaged => Unsafe.As<Vector2, Vector2<T>>(ref vector);

        /// <inheritdoc cref="AsGeneric{T}(System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> AsGeneric<T>(this Vector3 vector) where T : unmanaged => Unsafe.As<Vector3, Vector3<T>>(ref vector);

        /// <inheritdoc cref="AsGeneric{T}(System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AsGeneric<T>(this Vector4 vector) where T : unmanaged => Unsafe.As<Vector4, Vector4<T>>(ref vector);

        #endregion

        #endregion
    }
}
