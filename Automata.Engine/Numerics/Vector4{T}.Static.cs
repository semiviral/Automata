using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Intrinsic = System.Numerics.Vector;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector4<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Add(Vector4<T> a, Vector4<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Add(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Add(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector4<ulong, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector4 result = a.AsRef<T, Vector4>() + b.AsRef<T, Vector4>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Add(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Subtract(Vector4<T> a, Vector4<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Subtract(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Subtract(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector4<ulong, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector4 result = a.AsRef<T, Vector4>() - b.AsRef<T, Vector4>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Subtract(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Multiply(Vector4<T> a, Vector4<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)))
            {
                // no byte-by-byte SIMD multiply, so defer to intrinsics
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector4();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong)))
            {
                // no long-sized SIMD multiply instruction, so defer to intrinsic
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector4();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector4 result = a.AsRef<T, Vector4>() * b.AsRef<T, Vector4>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Multiply(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Divide(Vector4<T> a, Vector4<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector4 result = a.AsRef<T, Vector4>() * b.AsRef<T, Vector4>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Divide(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                // divide only has floating point instructions, so defer to intrinsics
                return (a.AsVectorRef() / b.AsVectorRef()).AsVector4();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> And(Vector4<T> a, Vector4<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector4<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector4<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.And(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Or(Vector4<T> a, Vector4<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector4<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector4<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector4<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector4<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector4<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector4<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Or(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector4<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Abs(Vector4<T> a)
        {
            if ((typeof(T) == typeof(sbyte)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, sbyte>()).AsVector4<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, short>()).AsVector4<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, int>()).AsVector4<uint, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                return Vector4.Abs(a.AsRef<T, Vector4>()).AsGeneric<T>();
            }
            else if ((typeof(T) == typeof(double)) || (typeof(T) == typeof(long)))
            {
                return Intrinsic.Abs(a.AsVectorRef()).AsVector4();
            }
            else
            {
                return a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Floor(Vector4<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Floor(a.AsVector128Ref<T, float>()).AsVector4<float, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector4<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Avx.IsSupported
                    ? Avx.Floor(a.AsVector256Ref<T, double>()).AsVector4<double, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector4<float, T>();
            }
            else
            {
                return a;
            }
        }


        #region Comparison

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> Equals(Vector4<T> a, Vector4<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector4<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> NotEquals(Vector4<T> a, Vector4<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                Vector128<sbyte> result = Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>());
                return Sse2.AndNot(result, Vector128<sbyte>.AllBitsSet).AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> result = Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>());
                return Sse2.AndNot(result, Vector128<short>.AllBitsSet).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> result = Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>());
                return Sse2.AndNot(result, Vector128<int>.AllBitsSet).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef())).BooleanReduce4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> result = Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>());
                return Sse.AndNot(result, Vector128<float>.AllBitsSet).Reduce().AsVector4<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                Vector256<double> result = Avx.CompareEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>());
                return Avx.AndNot(result, Vector256<double>.AllBitsSet).Reduce().AsVector4<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> GreaterThan(Vector4<T> a, Vector4<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.GreaterThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareGreaterThan(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector4<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> LessThan(Vector4<T> a, Vector4<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.LessThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce4();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector4<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareLessThan(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector4<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4<bool> GreaterThanOrEqual(Vector4<T> a, Vector4<T> b) =>
            Intrinsic.GreaterThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce4();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4<bool> LessThanOrEqual(Vector4<T> a, Vector4<T> b) => Intrinsic.LessThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce4();

        #endregion
    }
}
