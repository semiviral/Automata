using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public static class Vector
    {
        internal static void ThrowNotSupportedType() => throw new NotSupportedException("Given type is not supported.");


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


        #region Reduce

        /// <summary>
        ///     Reduces a given <see cref="Vector128" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> Reduce<TType>(this Vector128<TType> a) where TType : unmanaged
        {
            if (((typeof(TType) == typeof(sbyte)) || (typeof(TType) == typeof(byte)) || (typeof(TType) == typeof(bool))) && Sse2.IsSupported)
            {
                return a.AsRef<TType, sbyte>();
            }
            else if (((typeof(TType) == typeof(short)) || (typeof(TType) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<sbyte> a8 = Sse2.PackSignedSaturate(a.AsRef<TType, short>(), Vector128<short>.Zero);
                return a8;
            }
            else if (((typeof(TType) == typeof(int)) || (typeof(TType) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<short> a16 = Sse2.PackSignedSaturate(a.AsRef<TType, int>(), Vector128<int>.Zero);
                Vector128<sbyte> a8 = Sse2.PackSignedSaturate(a16, Vector128<short>.Zero);
                return a8;
            }
            else if ((typeof(TType) == typeof(float)) && Sse2.IsSupported)
            {
                Vector128<int> a32 = Sse2.ConvertToVector128Int32(a.As<TType, float>());
                Vector128<short> a16 = Sse2.PackSignedSaturate(a32, Vector128<int>.Zero);
                Vector128<sbyte> a8 = Sse2.PackSignedSaturate(a16, Vector128<short>.Zero);
                return a8;
            }
            else if (typeof(TType) == typeof(double))
            {
                Vector128<int> a32 = Sse2.ConvertToVector128Int32(a.AsRef<TType, double>());
                Vector128<short> a16 = Sse2.PackSignedSaturate(a32, Vector128<int>.Zero);
                Vector128<sbyte> a8 = Sse2.PackSignedSaturate(a16, Vector128<short>.Zero);
                return a8;
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        /// <summary>
        ///     Reduces a given <see cref="Vector128" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<sbyte> Reduce<TType>(this Vector256<TType> a) where TType : unmanaged
        {
            if ((typeof(TType) == typeof(sbyte)) || (typeof(TType) == typeof(byte)) || (typeof(TType) == typeof(bool)))
            {
                return a.AsRef<TType, sbyte>();
            }
            else if (((typeof(TType) == typeof(short)) || (typeof(TType) == typeof(ushort))) && Avx2.IsSupported)
            {
                Vector256<sbyte> a8 = Avx2.PackSignedSaturate(a.AsRef<TType, short>(), Vector256<short>.Zero);
                return a8;
            }
            else if (((typeof(TType) == typeof(int)) || (typeof(TType) == typeof(uint))) && Avx2.IsSupported)
            {
                Vector256<short> a16 = Avx2.PackSignedSaturate(a.AsRef<TType, int>(), Vector256<int>.Zero);
                Vector256<sbyte> a8 = Avx2.PackSignedSaturate(a16, Vector256<short>.Zero);
                return a8;
            }
            else if ((typeof(TType) == typeof(float)) && Avx2.IsSupported)
            {
                Vector256<int> a32 = Avx.ConvertToVector256Int32(a.As<TType, float>());
                Vector256<short> a16 = Avx2.PackSignedSaturate(a32, Vector256<int>.Zero);
                Vector256<sbyte> a8 = Avx2.PackSignedSaturate(a16, Vector256<short>.Zero);
                return a8;
            }
            else if ((typeof(TType) == typeof(double)) && Avx2.IsSupported)
            {
                Vector128<int> a32 = Avx.ConvertToVector128Int32(a.As<TType, double>());
                Vector128<short> a16 = Sse2.PackSignedSaturate(a32, Vector128<int>.Zero);
                Vector128<sbyte> a8 = Sse2.PackSignedSaturate(a16, Vector128<short>.Zero);
                return Unsafe.As<Vector128<sbyte>, Vector256<sbyte>>(ref a8);
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        public static unsafe Vector4<bool> BooleanReduce<T>(this Vector<T> a) where T : unmanaged
        {
            byte* pointer = (byte*)&a;

            return new Vector4<bool>(
                *(pointer + (sizeof(T) * 0)) != default,
                *(pointer + (sizeof(T) * 1)) != default,
                *(pointer + (sizeof(T) * 2)) != default,
                *(pointer + (sizeof(T) * 3)) != default);
        }

        #endregion


        #region Arbitrary Conversions

        public static Point AsPoint(this Vector2<int> vector) => Unsafe.As<Vector2<int>, Point>(ref vector);
        public static Vector2<int> AsVector(this Point point) => Unsafe.As<Point, Vector2<int>>(ref point);

        public static PointF AsPointF(this Vector2<float> vector) => Unsafe.As<Vector2<float>, PointF>(ref vector);
        public static Vector2<float> AsVector(this PointF pointF) => Unsafe.As<PointF, Vector2<float>>(ref pointF);

        public static Size AsSize(this Vector2<int> vector) => Unsafe.As<Vector2<int>, Size>(ref vector);
        public static Vector2<int> AsVector(this Size size) => Unsafe.As<Size, Vector2<int>>(ref size);

        #endregion


        #region Reinterpret

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

        #endregion


        #region As

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


        #region AsRef

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo AsRef<TFrom, TTo>(ref this Vector2<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector2<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo AsRef<TFrom, TTo>(ref this Vector3<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector3<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo AsRef<TFrom, TTo>(ref this Vector4<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector4<TFrom>, TTo>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector128<TTo> AsRef<TFrom, TTo>(ref this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector128<TFrom>, Vector128<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector256<TTo> AsRef<TFrom, TTo>(ref this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector256<TFrom>, Vector256<TTo>>(ref vector);

        #endregion


        #region AsVector

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


        #region AsVectorRef

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVectorRef<T>(ref this Vector2<T> vector) where T : unmanaged => ref Unsafe.As<Vector2<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVectorRef<T>(ref this Vector3<T> vector) where T : unmanaged => ref Unsafe.As<Vector3<T>, Vector<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector<T> AsVectorRef<T>(ref this Vector4<T> vector) where T : unmanaged => ref Unsafe.As<Vector4<T>, Vector<T>>(ref vector);

        #endregion


        #region AsVector2/3/4

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AsVector4<T>(this Vector<T> vector) where T : unmanaged => Unsafe.As<Vector<T>, Vector4<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector128<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<TTo> AsVector4<TFrom, TTo>(this Vector256<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            Unsafe.As<Vector256<TFrom>, Vector4<TTo>>(ref vector);

        #endregion


        #region AsVector2/3/4Ref

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2<T> AsVector2Ref<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector2<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2<TTo> AsVector2Ref<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector2<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<T> AsVector3Ref<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector3<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<TTo> AsVector3Ref<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector3<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<T> AsVector4Ref<T>(ref this Vector<T> vector) where T : unmanaged => ref Unsafe.As<Vector<T>, Vector4<T>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<TTo> AsVector4Ref<TFrom, TTo>(ref this Vector<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector<TFrom>, Vector4<TTo>>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<TTo> AsVector4Ref<TFrom, TTo>(ref this Vector128<TFrom> vector)
            where TFrom : unmanaged
            where TTo : unmanaged =>
            ref Unsafe.As<Vector128<TFrom>, Vector4<TTo>>(ref vector);

        #endregion


        #region AsVector128Ref

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector128<TTo> AsVector128Ref<TFrom, TTo>(ref this Vector4<TFrom> a)
            where TFrom : unmanaged
            where TTo : unmanaged
            => ref Unsafe.As<Vector4<TFrom>, Vector128<TTo>>(ref a);

        #endregion


        #region AsVector256Ref

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector256<TTo> AsVector256Ref<TFrom, TTo>(ref this Vector4<TFrom> a)
            where TFrom : unmanaged
            where TTo : unmanaged
            => ref Unsafe.As<Vector4<TFrom>, Vector256<TTo>>(ref a);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AsIntrinsic(this Vector3<float> vector) => Unsafe.As<Vector3<float>, Vector3>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 AsIntrinsic(this Vector4<float> vector) => Unsafe.As<Vector4<float>, Vector4>(ref vector);

        #endregion


        #region AsIntrinsicRef

        /// <summary>
        ///     Converts a given generic vector to its intrinsic variant.
        /// </summary>
        /// <remarks>
        ///     It's assumed that T is a valid type. No type checking is done by this method for performance.
        /// </remarks>
        /// <param name="vector">Vector to convert.</param>
        /// <returns>Intrinsic variant of the given vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector2 AsIntrinsicRef(ref this Vector2<float> vector) => ref Unsafe.As<Vector2<float>, Vector2>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3 AsIntrinsicRef(ref this Vector3<float> vector) => ref Unsafe.As<Vector3<float>, Vector3>(ref vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4 AsIntrinsicRef(ref this Vector4<float> vector) => ref Unsafe.As<Vector4<float>, Vector4>(ref vector);

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

        /// <inheritdoc cref="AsGenericRef{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> AsGeneric<T>(this Vector3 vector) where T : unmanaged => Unsafe.As<Vector3, Vector3<T>>(ref vector);

        /// <inheritdoc cref="AsGenericRef{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AsGeneric<T>(this Vector4 vector) where T : unmanaged => Unsafe.As<Vector4, Vector4<T>>(ref vector);

        #endregion


        #region AsGenericRef

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
        public static ref Vector2<T> AsGenericRef<T>(ref this Vector2 vector) where T : unmanaged => ref Unsafe.As<Vector2, Vector2<T>>(ref vector);

        /// <inheritdoc cref="AsGenericRef{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector3<T> AsGenericRef<T>(ref this Vector3 vector) where T : unmanaged => ref Unsafe.As<Vector3, Vector3<T>>(ref vector);

        /// <inheritdoc cref="AsGenericRef{T}(ref System.Numerics.Vector2)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Vector4<T> AsGenericRef<T>(ref this Vector4 vector) where T : unmanaged => ref Unsafe.As<Vector4, Vector4<T>>(ref vector);

        #endregion
    }
}
