using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Automata.Engine.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector3<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Add(Vector3<T> a, Vector3<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() + b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() + b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Subtract(Vector3<T> a, Vector3<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() - b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() - b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Multiply(Vector3<T> a, Vector3<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() * b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() * b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Divide(Vector3<T> a, Vector3<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector3>() * b.As<T, Vector3>()).AsGeneric<T>()
                : (a.AsVector() / b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> And(Vector3<T> a, Vector3<T> b) => System.Numerics.Vector.BitwiseAnd(a.AsVector(), b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Or(Vector3<T> a, Vector3<T> b) => System.Numerics.Vector.BitwiseOr(a.AsVector(), b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Abs(Vector3<T> a) => System.Numerics.Vector.Abs(a.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Floor(Vector3<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return System.Numerics.Vector.Floor(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return System.Numerics.Vector.Floor(a.AsVector<T, double>()).AsVector3<double, T>();
            }
            else
            {
                return a;
            }
        }


        #region Comparison

        /// <summary>
        ///     Reduces a given <see cref="Vector128" /> to booleans representing each of its elements.
        /// </summary>
        /// <param name="a"><see cref="Vector128{T}" /> to reduce.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> BooleanReduction<TType>(Vector128<TType> a) where TType : unmanaged
        {
            if (((typeof(TType) == typeof(int)) || (typeof(TType) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<short> intermediate = Sse2.PackSignedSaturate(a.As<TType, int>(), Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else if (((typeof(TType) == typeof(short)) || (typeof(TType) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.PackSignedSaturate(a.As<TType, short>(), Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else if ((typeof(TType) == typeof(sbyte)) || (typeof(TType) == typeof(byte)))
            {
                return a.AsVector3<TType, bool>();
            }
            else if ((typeof(TType) == typeof(float)) && Sse2.IsSupported)
            {
                Vector128<int> converted = Sse2.ConvertToVector128Int32(a.As<TType, float>());
                Vector128<short> intermediate = Sse2.PackSignedSaturate(converted, Vector128<int>.Zero);
                return Sse2.PackSignedSaturate(intermediate, Vector128<short>.Zero).AsVector3<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> Equals(Vector3<T> a, Vector3<T> b)
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>()));
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>()));
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()));
            }
            else if ((typeof(T) == typeof(float)) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>()));
            }
            else if (typeof(T) == typeof(double))
            {
                return new Vector3<bool>(
                    a.X.Coerce<T, double>() == b.X.Coerce<T, double>(),
                    a.Y.Coerce<T, double>() == b.Y.Coerce<T, double>(),
                    a.Z.Coerce<T, double>() == b.Z.Coerce<T, double>());
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> NotEquals(Vector3<T> a, Vector3<T> b)
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> compareEqual = Sse2.CompareEqual(a.AsVector128<T, int>(), b.AsVector128<T, int>());
                return BooleanReduction(Sse2.AndNot(compareEqual, Vector128<int>.AllBitsSet));
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> compareEqual = Sse2.CompareEqual(a.AsVector128<T, short>(), b.AsVector128<T, short>());
                return BooleanReduction(Sse2.AndNot(compareEqual, Vector128<short>.AllBitsSet));
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                Vector128<sbyte> compareEqual = Sse2.CompareEqual(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>());
                return BooleanReduction(Sse2.AndNot(compareEqual, Vector128<sbyte>.AllBitsSet));
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> compareEqual = Sse.CompareEqual(a.AsVector128<T, float>(), b.AsVector128<T, float>());
                return BooleanReduction(Sse.AndNot(compareEqual, Vector128<float>.AllBitsSet));
            }
            else if (typeof(T) == typeof(double))
            {
                return new Vector3<bool>(
                    a.X.Coerce<T, double>() != b.X.Coerce<T, double>(),
                    a.Y.Coerce<T, double>() != b.Y.Coerce<T, double>(),
                    a.Z.Coerce<T, double>() != b.Z.Coerce<T, double>());
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> GreaterThan(Vector3<T> a, Vector3<T> b)
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareGreaterThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()));
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareGreaterThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()));
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareGreaterThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()));
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return BooleanReduction(Sse.CompareGreaterThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()));
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> LessThan(Vector3<T> a, Vector3<T> b)
        {
            if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareLessThan(a.AsVector128<T, int>(), b.AsVector128<T, int>()));
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareLessThan(a.AsVector128<T, short>(), b.AsVector128<T, short>()));
            }
            else if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte))) && Sse2.IsSupported)
            {
                return BooleanReduction(Sse2.CompareLessThan(a.AsVector128<T, sbyte>(), a.AsVector128<T, sbyte>()));
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return BooleanReduction(Sse.CompareLessThan(a.AsVector128<T, float>(), b.AsVector128<T, float>()));
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        private static Vector3<bool> GreaterThanOrEqual(in Vector3<T> vector3, in Vector3<T> vector4) { throw new NotImplementedException(); }
        private static Vector3<bool> LessThanOrEqual(in Vector3<T> vector3, in Vector3<T> vector4) { throw new NotImplementedException(); }

        #endregion


    }
}
