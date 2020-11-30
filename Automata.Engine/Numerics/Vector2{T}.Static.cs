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
    public readonly partial struct Vector2<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Add(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, long>(), b.AsVector128Ref<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, ulong>(), b.AsVector128Ref<T, ulong>()).AsVector2<ulong, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector2 result = a.AsRef<T, Vector2>() + b.AsRef<T, Vector2>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Add(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Subtract(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, long>(), b.AsVector128Ref<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, ulong>(), b.AsVector128Ref<T, ulong>()).AsVector2<ulong, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector2 result = a.AsRef<T, Vector2>() - b.AsRef<T, Vector2>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Subtract(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Multiply(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)))
            {
                // no byte-by-byte SIMD multiply, so defer to intrinsics
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector2();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.MultiplyLow(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse41.MultiplyLow(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong)))
            {
                // no long-sized SIMD multiply instruction, so defer to intrinsic
                return (a.AsVectorRef() * b.AsVectorRef()).AsVector2();
            }
            else if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector2 result = a.AsRef<T, Vector2>() * b.AsRef<T, Vector2>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Multiply(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Divide(Vector2<T> a, Vector2<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                // just defer to intrinsics for floats, they're faster
                Vector2 result = a.AsRef<T, Vector2>() * b.AsRef<T, Vector2>();
                return result.AsGenericRef<T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Divide(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                // divide only has floating point instructions, so defer to intrinsics
                return (a.AsVectorRef() / b.AsVectorRef()).AsVector2();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> And(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, long>(), b.AsVector128Ref<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, ulong>(), b.AsVector128Ref<T, ulong>()).AsVector2<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.And(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector2<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.And(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Or(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(sbyte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, T>();
            }
            else if ((typeof(T) == typeof(byte)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, byte>(), b.AsVector128Ref<T, byte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).AsVector2<short, T>();
            }
            else if ((typeof(T) == typeof(ushort)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, ushort>(), b.AsVector128Ref<T, ushort>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).AsVector2<int, T>();
            }
            else if ((typeof(T) == typeof(uint)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, uint>(), b.AsVector128Ref<T, uint>()).AsVector2<uint, T>();
            }
            else if ((typeof(T) == typeof(long)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, long>(), b.AsVector128Ref<T, long>()).AsVector2<long, T>();
            }
            else if ((typeof(T) == typeof(ulong)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, ulong>(), b.AsVector128Ref<T, ulong>()).AsVector2<ulong, T>();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.Or(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).AsVector2<float, T>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.Or(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).AsVector2<double, T>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Abs(Vector2<T> a)
        {
            if ((typeof(T) == typeof(sbyte)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, sbyte>()).AsVector2<byte, T>();
            }
            else if ((typeof(T) == typeof(short)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, short>()).AsVector2<ushort, T>();
            }
            else if ((typeof(T) == typeof(int)) && Ssse3.IsSupported)
            {
                return Ssse3.Abs(a.AsVector128Ref<T, int>()).AsVector2<uint, T>();
            }
            else if (typeof(T) == typeof(float))
            {
                return Vector2.Abs(a.AsRef<T, Vector2>()).AsGeneric<T>();
            }
            else if ((typeof(T) == typeof(double)) || (typeof(T) == typeof(long)))
            {
                return Intrinsic.Abs(a.AsVectorRef()).AsVector2();
            }
            else
            {
                return a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Floor(Vector2<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Floor(a.AsVector128Ref<T, float>()).AsVector2<float, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Sse41.IsSupported
                    ? Sse41.Floor(a.AsVector128Ref<T, double>()).AsVector2<double, T>()
                    : Intrinsic.Floor(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else
            {
                return a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Ceiling(Vector2<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Sse41.IsSupported
                    ? Sse41.Ceiling(a.AsVector128Ref<T, float>()).AsVector2<float, T>()
                    : Intrinsic.Ceiling(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Sse41.IsSupported
                    ? Sse41.Ceiling(a.AsVector128Ref<T, double>()).AsVector2<double, T>()
                    : Intrinsic.Ceiling(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else
            {
                return a;
            }
        }


        #region Comparison

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> Equals(Vector2<T> a, Vector2<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.CompareEqual(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> NotEquals(Vector2<T> a, Vector2<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                Vector128<sbyte> result = Sse2.CompareEqual(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>());
                return Sse2.AndNot(result, Vector128<sbyte>.AllBitsSet).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                Vector128<short> result = Sse2.CompareEqual(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>());
                return Sse2.AndNot(result, Vector128<short>.AllBitsSet).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                Vector128<int> result = Sse2.CompareEqual(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>());
                return Sse2.AndNot(result, Vector128<int>.AllBitsSet).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef())).BooleanReduce2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                Vector128<float> result = Sse.CompareEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>());
                return Sse.AndNot(result, Vector128<float>.AllBitsSet).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                Vector128<double> result = Sse2.CompareEqual(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>());
                return Sse2.AndNot(result, Vector128<double>.AllBitsSet).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> GreaterThan(Vector2<T> a, Vector2<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.GreaterThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThan(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> LessThan(Vector2<T> a, Vector2<T> b)
        {
            if (((typeof(T) == typeof(sbyte)) || (typeof(T) == typeof(byte)) || (typeof(T) == typeof(bool))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, sbyte>(), b.AsVector128Ref<T, sbyte>()).AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, short>(), b.AsVector128Ref<T, short>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, int>(), b.AsVector128Ref<T, int>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if (((typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))) && Avx2.IsSupported)
            {
                // we can't reduce longs, so just convert to intrinsic vectors
                return Intrinsic.LessThan(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce2();
            }
            else if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThan(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThan(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                Vector.ThrowNotSupportedType();
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2<bool> GreaterThanOrEqual(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareGreaterThanOrEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.CompareGreaterThanOrEqual(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                return Intrinsic.GreaterThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce2();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2<bool> LessThanOrEqual(Vector2<T> a, Vector2<T> b)
        {
            if ((typeof(T) == typeof(float)) && Sse.IsSupported)
            {
                return Sse.CompareLessThanOrEqual(a.AsVector128Ref<T, float>(), b.AsVector128Ref<T, float>()).Reduce().AsVector2<sbyte, bool>();
            }
            else if ((typeof(T) == typeof(double)) && Sse2.IsSupported)
            {
                return Sse2.CompareLessThanOrEqual(a.AsVector128Ref<T, double>(), b.AsVector128Ref<T, double>()).Reduce().AsVector2<sbyte, bool>();
            }
            else
            {
                return Intrinsic.LessThanOrEqual(a.AsVectorRef(), b.AsVectorRef()).BooleanReduce2();
            }
        }

        #endregion
    }
}
