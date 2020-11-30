using System;
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
    public readonly partial struct Vector3<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Add(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Add(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Add(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Add(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Add(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Subtract(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Subtract(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Subtract(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Subtract(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Subtract(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Multiply(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)))
            {
                // no byte-by-byte SIMD multiply, so defer to intrinsics
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector3();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong)))
            {
                // no long-sized SIMD multiply instruction, so defer to intrinsic
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Multiply(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Multiply(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Divide(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Divide(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Divide(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                return new Vector3<T>(
                    Primitive<T>.Divide(a.X, b.X),
                    Primitive<T>.Divide(a.Y, b.Y),
                    Primitive<T>.Divide(a.Z, b.Z));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> And(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.And(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.And(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Or(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector3<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector3<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector3<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256Ref<T, long>(), b.AsVector256Ref<T, long>()).AsVector3<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Avx2.IsSupported)
            {
                return Avx2.Or(a.AsVector256Ref<T, ulong>(), b.AsVector256Ref<T, ulong>()).AsVector3<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector3<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.Or(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).AsVector3<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Abs(Vector3<T> a)
        {
            if ((typeof(T) == typeof(sbyte)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, sbyte>()).AsVector3<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, short>()).AsVector3<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, int>()).AsVector3<uint, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                return Vector3.Abs(a.AsRef<T, Vector3>()).AsGeneric<T>();
            }
            else if ((typeof(T) == typeof(double)) || (typeof(T) == typeof(long)))
            {
                return Intrinsic.Abs(a.AsVectorRef()).AsVector3();
            }
            else
            {
                return a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Floor(Vector3<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Floor(a.AsVector128Ref<T, float>()).AsVector3<float, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Avx.IsSupported
                    ? Avx.Floor(a.AsVector256Ref<T, double>()).AsVector3<double, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else
            {
                return a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Ceiling(Vector3<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Ceiling(a.AsVector128Ref<T, float>()).AsVector3<float, T>()
                    : Intrinsic.Ceiling(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Avx.IsSupported
                    ? Avx.Ceiling(a.AsVector256Ref<T, double>()).AsVector3<double, T>()
                    : Intrinsic.Ceiling(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else
            {
                return a;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> RoundBy(Vector3<T> a, Vector3<T> by) => (a / by) * by;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> RoundBy(Vector3<T> a, T by) => (a / by) * by;

        #region Comparison

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> Equals(Vector3<T> a, Vector3<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector3<sbyte, bool>();
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
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                Vector128<sbyte> result = Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>());
                return Sse2.AndNot(result, Vector128<sbyte>.AllBitsSet).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> result = Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>());
                return Sse2.AndNot(result, Vector128<short>.AllBitsSet).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> result = Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>());
                return Sse2.AndNot(result, Vector128<int>.AllBitsSet).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef())).BooleanReduce3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> result = Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>());
                return Sse.AndNot(result, Vector128<float>.AllBitsSet).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                Vector256<double> result = Avx.CompareEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>());
                return Avx.AndNot(result, Vector256<double>.AllBitsSet).Reduce().AsVector3<sbyte, bool>();
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
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.GreaterThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareGreaterThan(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector3<sbyte, bool>();
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
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.LessThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce3();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareLessThan(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector3<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> GreaterThanOrEqual(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThanOrEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareGreaterThanOrEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector3<sbyte, bool>();
            }
            else
            {
                return Intrinsic.GreaterThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce3();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> LessThanOrEqual(Vector3<T> a, Vector3<T> b)
        {
            if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThanOrEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector3<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Avx.IsSupported)
            {
                return Avx.CompareLessThanOrEqual(a.AsVector256Ref<T, double>(), b.AsVector256Ref<T, double>()).Reduce().AsVector3<sbyte, bool>();
            }
            else
            {
                return Intrinsic.LessThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce3();
            }
        }

        #endregion
    }
}
