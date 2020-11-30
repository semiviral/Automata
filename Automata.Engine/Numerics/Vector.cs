using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public static class Vector
    {
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


        #region Arbitrary Conversions

        public static ref Point AsPoint(ref this Vector2<int> vector) => ref Unsafe.As<Vector2<int>, Point>(ref vector);
        public static ref Vector2<int> AsVector(ref this Point point) => ref Unsafe.As<Point, Vector2<int>>(ref point);

        public static ref PointF AsPointF(ref this Vector2<float> vector) => ref Unsafe.As<Vector2<float>, PointF>(ref vector);
        public static ref Vector2<float> AsVector(ref this PointF pointF) => ref Unsafe.As<PointF, Vector2<float>>(ref pointF);

        public static ref Size AsSize(ref this Vector2<int> vector) => ref Unsafe.As<Vector2<int>, Size>(ref vector);
        public static ref Vector2<int> AsVector(ref this Size size) => ref Unsafe.As<Size, Vector2<int>>(ref size);

        #endregion


        #region Vector2/3/4 As

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2<TTo> Reinterpret<TFrom, TTo>(ref this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector2<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<TTo> Reinterpret<TFrom, TTo>(ref this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector3<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<TTo> Reinterpret<TFrom, TTo>(ref this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector4<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector2<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector3<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector4<TFrom>, TTo>(ref vector);

        #endregion


        #region AsVector

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVector<T>(ref this Vector2<T> vector) where T : unmanaged => ref Unsafe.As<Vector2<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVector<T>(ref this Vector3<T> vector) where T : unmanaged => ref Unsafe.As<Vector3<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVector<T>(ref this Vector4<T> vector) where T : unmanaged => ref Unsafe.As<Vector4<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<TTo> AsVector<TFrom, TTo>(ref this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector2<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<TTo> AsVector<TFrom, TTo>(ref this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector3<TFrom>, Vector<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<TTo> AsVector<TFrom, TTo>(ref this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector4<TFrom>, Vector<TTo>>(ref vector);

        #endregion


        #region AsVector2/3/4

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2<T> AsVector2<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector2<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<T> AsVector3<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector3<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<T> AsVector4<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector4<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2<TTo> AsVector2<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<TTo> AsVector3<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<TTo> AsVector4<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector4<TTo>>(ref vector);

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
        public static ref Vector2 AsIntrinsic(ref this Vector2<float> vector) => ref Unsafe.As<Vector2<float>, Vector2>(ref vector);

        /// <inheritdoc cref="AsIntrinsic(ref Automata.Engine.Numerics.Vector2{float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3 AsIntrinsic(ref this Vector3<float> vector) => ref Unsafe.As<Vector3<float>, Vector3>(ref vector);

        /// <inheritdoc cref="AsIntrinsic(ref Automata.Engine.Numerics.Vector2{float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4 AsIntrinsic(ref this Vector4<float> vector) => ref Unsafe.As<Vector4<float>, Vector4>(ref vector);

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
        public static ref Vector2<T> AsGeneric<T>(ref this Vector2 vector) where T : unmanaged => ref Unsafe.As<Vector2, Vector2<T>>(ref vector);

        /// <inheritdoc cref="AsGeneric{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<T> AsGeneric<T>(ref this Vector3 vector) where T : unmanaged => ref Unsafe.As<Vector3, Vector3<T>>(ref vector);

        /// <inheritdoc cref="AsGeneric{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<T> AsGeneric<T>(ref this Vector4 vector) where T : unmanaged => ref Unsafe.As<Vector4, Vector4<T>>(ref vector);

        #endregion
    }
}
