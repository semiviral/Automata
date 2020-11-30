using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
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
            if (typeof(T) == typeof(float))
            {
                Vector4 result = a.As<T, Vector4>() + b.As<T, Vector4>();
                return result.AsGeneric<T>();
            }
            else
            {
                Vector<T> result = a.AsVector() + b.AsVector();
                return result.AsVector4();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Subtract(Vector4<T> a, Vector4<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector4 result = a.As<T, Vector4>() - b.As<T, Vector4>();
                return result.AsGeneric<T>();
            }
            else
            {
                Vector<T> result = a.AsVector() - b.AsVector();
                return result.AsVector4();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Multiply(Vector4<T> a, Vector4<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector4 result = a.As<T, Vector4>() * b.As<T, Vector4>();
                return result.AsGeneric<T>();
            }
            else
            {
                Vector<T> result = a.AsVector() * b.AsVector();
                return result.AsVector4();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Divide(Vector4<T> a, Vector4<T> b)
        {
            if (typeof(T) == typeof(float))
            {
                Vector4 result = a.As<T, Vector4>() / b.As<T, Vector4>();
                return result.AsGeneric<T>();
            }
            else
            {
                Vector<T> result = a.AsVector() / b.AsVector();
                return result.AsVector4();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> And(Vector4<T> a, Vector4<T> b)
        {
            Vector<T> result = Intrinsic.BitwiseAnd(a.AsVector(), b.AsVector());
            return result.AsVector4();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Or(Vector4<T> a, Vector4<T> b)
        {
            Vector<T> result = Intrinsic.BitwiseOr(a.AsVector(), b.AsVector());
            return result.AsVector4();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Abs(Vector4<T> a)
        {
            Vector<T> result = Intrinsic.Abs(a.AsVector());
            return result.AsVector4();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<T> Floor(Vector4<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                Vector<float> result = Intrinsic.Floor(a.AsVector<T, float>());
                return result.AsVector4<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                Vector<double> result = Intrinsic.Floor(a.AsVector<T, double>());
                return result.AsVector4<double, T>();
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
        private static Vector4<bool> BooleanReduction<TType>(Vector<TType> a) where TType : unmanaged =>
            new Vector4<bool>(
                !a[0].Equals(default),
                !a[1].Equals(default),
                !a[2].Equals(default),
                !a[3].Equals(default));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> Equals(Vector4<T> a, Vector4<T> b) => BooleanReduction(Intrinsic.Equals(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> NotEquals(Vector4<T> a, Vector4<T> b) =>
            BooleanReduction(Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVector(), b.AsVector())));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> GreaterThan(Vector4<T> a, Vector4<T> b) => BooleanReduction(Intrinsic.GreaterThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4<bool> LessThan(Vector4<T> a, Vector4<T> b) => BooleanReduction(Intrinsic.LessThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4<bool> GreaterThanOrEqual(Vector4<T> a, Vector4<T> b) =>
            BooleanReduction(Intrinsic.GreaterThanOrEqual(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4<bool> LessThanOrEqual(Vector4<T> a, Vector4<T> b) => BooleanReduction(Intrinsic.LessThanOrEqual(a.AsVector(), b.AsVector()));

        #endregion
    }
}
