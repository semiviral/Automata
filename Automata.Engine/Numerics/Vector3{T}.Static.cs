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
        public static Vector3<T> And(Vector3<T> a, Vector3<T> b) => Intrinsic.BitwiseAnd(a.AsVector(), b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Or(Vector3<T> a, Vector3<T> b) => Intrinsic.BitwiseOr(a.AsVector(), b.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Abs(Vector3<T> a) => Intrinsic.Abs(a.AsVector()).AsVector3();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<T> Floor(Vector3<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Intrinsic.Floor(a.AsVector<T, float>()).AsVector3<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Intrinsic.Floor(a.AsVector<T, double>()).AsVector3<double, T>();
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
        public static Vector3<bool> Equals(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.Equals(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> NotEquals(Vector3<T> a, Vector3<T> b) =>
            BooleanReduction(Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVector(), b.AsVector())));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> GreaterThan(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.GreaterThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3<bool> LessThan(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.LessThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> GreaterThanOrEqual(Vector3<T> a, Vector3<T> b) =>
            BooleanReduction(Intrinsic.GreaterThanOrEqual(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3<bool> LessThanOrEqual(Vector3<T> a, Vector3<T> b) => BooleanReduction(Intrinsic.LessThanOrEqual(a.AsVector(), b.AsVector()));

        #endregion
    }
}
