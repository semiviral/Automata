using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
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
            if (typeof(T) == typeof(float))
            {
                Vector3 result = a.AsRef<T, Vector3>() + b.AsRef<T, Vector3>();
                return result.AsGenericRef<T>();
            }
            else
            {
                Vector<T> result = a.AsVectorRef() + b.AsVectorRef();
                return result.AsVector3Ref();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Subtract(Vector3<T> a, Vector3<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector3 result = a.AsRef<T, Vector3>() - b.AsRef<T, Vector3>();
                return result.AsGenericRef<T>();
            }
            else
            {
                Vector<T> result = a.AsVectorRef() - b.AsVectorRef();
                return result.AsVector3Ref();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Multiply(Vector3<T> a, Vector3<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector3 result = a.AsRef<T, Vector3>() * b.AsRef<T, Vector3>();
                return result.AsGenericRef<T>();
            }
            else
            {
                Vector<T> result = a.AsVectorRef() * b.AsVectorRef();
                return result.AsVector3Ref();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Divide(Vector3<T> a, Vector3<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector3 result = a.AsRef<T, Vector3>() / b.AsRef<T, Vector3>();
                return result.AsGenericRef<T>();
            }
            else
            {
                Vector<T> result = a.AsVectorRef() / b.AsVectorRef();
                return result.AsVector3Ref();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> And(Vector3<T> a, Vector3<T> b)
        {
            Vector<T> result = Intrinsic.BitwiseAnd(a.AsVectorRef(), b.AsVectorRef());
            return result.AsVector3Ref();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Or(Vector3<T> a, Vector3<T> b)
        {
            Vector<T> result = Intrinsic.BitwiseOr(a.AsVectorRef(), b.AsVectorRef());
            return result.AsVector3Ref();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Abs(Vector3<T> a)
        {
            Vector<T> result = Intrinsic.Abs(a.AsVectorRef());
            return result.AsVector3Ref();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Floor(Vector3<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                Vector<float> result = Intrinsic.Floor(a.AsVector<T, float>());
                return result.AsVector3Ref<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                Vector<double> result = Intrinsic.Floor(a.AsVector<T, double>());
                return result.AsVector3Ref<double, T>();
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
        private static Vector3<bool> BooleanReduction<TType>(Vector<TType> a) where TType : unmanaged =>
            new Vector3<bool>(
                !a[0].Equals(default),
                !a[1].Equals(default),
                !a[2].Equals(default));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> Equals(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> NotEquals(Vector3<T> a, Vector3<T> b) =>
            BooleanReduction(Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVectorRef(), b.AsVectorRef())));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> GreaterThan(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.GreaterThan(a.AsVectorRef(), b.AsVectorRef()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> LessThan(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.LessThan(a.AsVectorRef(), b.AsVectorRef()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> GreaterThanOrEqual(Vector3<T> a, Vector3<T> b) =>
            BooleanReduction(Intrinsic.GreaterThanOrEqual(a.AsVectorRef(), b.AsVectorRef()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> LessThanOrEqual(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.LessThanOrEqual(a.AsVectorRef(), b.AsVectorRef()));

        #endregion
    }
}
