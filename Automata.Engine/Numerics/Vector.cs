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


        #region Arithmetic

        /// <summary>
        ///     Reduces a given <see cref="Vector128{T}" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2<bool> BooleanReduction2<T>(this Vector128<T> a) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<short> intermediate = Sse2.PackSignedSaturate(a.As<T, int>(), Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.PackSignedSaturate(a.As<T, short>(), Vector128<short>.Zero).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return a.AsVector2<T, bool>();
            }
            else if ((typeof(T) == typeof(float)) && Sse2.IsSupported)
            {
                Vector128<int> converted = Sse2.ConvertToVector128Int32(a.Coerce<T, float>());
                Vector128<short> intermediate = Sse2.PackSignedSaturate(converted, Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector2<sbyte, bool>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        /// <summary>
        ///     Reduces a given <see cref="Vector128{T}" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> BooleanReduction3<T>(this Vector128<T> a) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<short> intermediate = Sse2.PackSignedSaturate(a.As<T, int>(), Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.PackSignedSaturate(a.As<T, short>(), Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return a.AsVector3<T, bool>();
            }
            else if ((typeof(T) == typeof(float)) && Sse2.IsSupported)
            {
                Vector128<int> converted = Sse2.ConvertToVector128Int32(a.Coerce<T, float>());
                Vector128<short> intermediate = Sse2.PackSignedSaturate(converted, Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        /// <summary>
        ///     Reduces a given <see cref="Vector128{T}" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4<bool> BooleanReduction4<T>(this Vector128<T> a) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<short> intermediate = Sse2.PackSignedSaturate(a.As<T, int>(), Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.PackSignedSaturate(a.As<T, short>(), Vector128<short>.Zero).AsVector4<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)))
            {
                return a.AsVector4<T, bool>();
            }
            else if ((typeof(T) == typeof(float)) && Sse2.IsSupported)
            {
                Vector128<int> converted = Sse2.ConvertToVector128Int32(a.As<T, float>());
                Vector128<short> intermediate = Sse2.PackSignedSaturate(converted, Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector4<sbyte, bool>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }


        #region Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<bool> EqualsInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction2();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> EqualsInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction3();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> EqualsInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction4();
            }
            else if ((typeof(T) == typeof(float)) && Sse2.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction4();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion


        #region Not Equals

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<bool> NotEqualsInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> compareEqual = Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>());
                return Sse2.AndNot(compareEqual, Vector128<int>.AllBitsSet).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> compareEqual = Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>());
                return Sse2.AndNot(compareEqual, Vector128<short>.AllBitsSet).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                Vector128<sbyte> compareEqual = Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>());
                return Sse2.AndNot(compareEqual, Vector128<sbyte>.AllBitsSet).BooleanReduction2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> compareEqual = Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>());
                return Sse.AndNot(compareEqual, Vector128<float>.AllBitsSet).BooleanReduction2();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> NotEqualsInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> compareEqual = Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>());
                return Sse2.AndNot(compareEqual, Vector128<int>.AllBitsSet).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> compareEqual = Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>());
                return Sse2.AndNot(compareEqual, Vector128<short>.AllBitsSet).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                Vector128<sbyte> compareEqual = Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>());
                return Sse2.AndNot(compareEqual, Vector128<sbyte>.AllBitsSet).BooleanReduction3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> compareEqual = Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>());
                return Sse.AndNot(compareEqual, Vector128<float>.AllBitsSet).BooleanReduction3();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> NotEqualsInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> compareEqual = Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>());
                return Sse2.AndNot(compareEqual, Vector128<int>.AllBitsSet).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> compareEqual = Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>());
                return Sse2.AndNot(compareEqual, Vector128<short>.AllBitsSet).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                Vector128<sbyte> compareEqual = Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>());
                return Sse2.AndNot(compareEqual, Vector128<sbyte>.AllBitsSet).BooleanReduction4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> compareEqual = Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>());
                return Sse.AndNot(compareEqual, Vector128<float>.AllBitsSet).BooleanReduction4();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion


        #region Add

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<T> AddInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged => (a.AsVector() + b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> AddInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged => (a.AsVector() + b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AddInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged => (a.AsVector() + b.AsVector()).AsVector4();

        #endregion


        #region Subtract

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<T> SubtractInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged => (a.AsVector() - b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> SubtractInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged => (a.AsVector() - b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> SubtractInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged => (a.AsVector() - b.AsVector()).AsVector4();

        #endregion


        #region Multiply

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<T> MultiplyInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() * b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() * b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> MultiplyInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() * b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() * b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> MultiplyInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            // these cases exist because they're more performant than Vector<T>
            if ((typeof(T) == typeof(int)) && Sse41.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return (a.As<T, Vector4>() * b.As<T, Vector4>()).AsGeneric<T>();
            }
            else
            {
                return (a.AsVector() * b.AsVector()).AsVector4();
            }
        }

        #endregion


        #region Divide

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<T> DivideInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() * b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() / b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> DivideInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() * b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() / b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> DivideInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged =>
            (typeof(T) == typeof(float)) && Sse.IsSupported
                ? (a.As<T, Vector4>() * b.As<T, Vector4>()).AsGeneric<T>()
                : (a.AsVector() / b.AsVector()).AsVector4();

        #endregion


        #region And

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> AndInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, long>(), b.AsVector128<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, ulong>(), b.AsVector128<T, ulong>()).AsVector2<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector2<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, double>(), b.AsVector128<T, double>()).AsVector2<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> AndInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256<T, long>(), b.AsVector256<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256<T, ulong>(), b.AsVector256<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.And(a.AsVector256<T, double>(), b.AsVector256<T, double>()).AsVector3<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> AndInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256<T, long>(), b.AsVector256<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256<T, ulong>(), b.AsVector256<T, ulong>()).AsVector4<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector4<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.And(a.AsVector256<T, double>(), b.AsVector256<T, double>()).AsVector4<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion


        #region Or

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> OrInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, long>(), b.AsVector128<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, ulong>(), b.AsVector128<T, ulong>()).AsVector2<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector2<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, double>(), b.AsVector128<T, double>()).AsVector2<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> OrInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256<T, long>(), b.AsVector256<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256<T, ulong>(), b.AsVector256<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Or(a.AsVector256<T, double>(), b.AsVector256<T, double>()).AsVector3<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> OrInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256<T, long>(), b.AsVector256<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256<T, ulong>(), b.AsVector256<T, ulong>()).AsVector4<ulong, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, int>(), b.AsVector128<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse41.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, uint>(), b.AsVector128<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, short>(), b.AsVector128<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, ushort>(), b.AsVector128<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, byte>(), b.AsVector128<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(bool)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128<T, sbyte>(), b.AsVector128<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128<T, float>(), b.AsVector128<T, float>()).AsVector4<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Or(a.AsVector256<T, double>(), b.AsVector256<T, double>()).AsVector4<double, T>();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion


        #region Greater Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<bool> GreaterThanInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction2();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3<bool> GreaterThanInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction3();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector4<bool> GreaterThanInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction4();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion


        #region Less Than

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2<bool> LessThanInternal<T>(Vector2<T> a, Vector2<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction2();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction2();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3<bool> LessThanInternal<T>(Vector3<T> a, Vector3<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction3();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction3();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector4<bool> LessThanInternal<T>(Vector4<T> a, Vector4<T> b) where T : unmanaged
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()).BooleanReduction4();
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()).BooleanReduction4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()).BooleanReduction4();
            }
            else
            {
                ThrowNotSupportedType();
                return default;
            }
        }

        #endregion

        #endregion
    }
}
