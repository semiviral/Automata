using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Intrinsic = System.Numerics.Vector;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable CognitiveComplexity

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector2<T> where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Add(Vector2<T> a, Vector2<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() + b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() + b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Subtract(Vector2<T> a, Vector2<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() - b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() - b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Multiply(Vector2<T> a, Vector2<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() * b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() * b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Divide(Vector2<T> a, Vector2<T> b) =>
            typeof(T) == typeof(float)
                ? (a.As<T, Vector2>() * b.As<T, Vector2>()).AsGeneric<T>()
                : (a.AsVector() / b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> And(Vector2<T> a, Vector2<T> b) => Intrinsic.BitwiseAnd(a.AsVector(), b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Or(Vector2<T> a, Vector2<T> b) => Intrinsic.BitwiseOr(a.AsVector(), b.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Abs(Vector2<T> a) => Intrinsic.Abs(a.AsVector()).AsVector2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<T> Floor(Vector2<T> a)
        {
            if (typeof(T) == typeof(float))
            {
                return Intrinsic.Floor(a.AsVector<T, float>()).AsVector2<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return Intrinsic.Floor(a.AsVector<T, double>()).AsVector2<double, T>();
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
        private static Vector2<bool> BooleanReduction<TType>(Vector<TType> a) where TType : unmanaged =>
            new Vector2<bool>(
                !a[0].Equals(default),
                !a[1].Equals(default));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> Equals(Vector2<T> a, Vector2<T> b) => BooleanReduction(Intrinsic.Equals(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> NotEquals(Vector2<T> a, Vector2<T> b) =>
            BooleanReduction(Intrinsic.OnesComplement(Intrinsic.Equals(a.AsVector(), b.AsVector())));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> GreaterThan(Vector2<T> a, Vector2<T> b) => BooleanReduction(Intrinsic.GreaterThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2<bool> LessThan(Vector2<T> a, Vector2<T> b) => BooleanReduction(Intrinsic.LessThan(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2<bool> GreaterThanOrEqual(Vector2<T> a, Vector2<T> b) =>
            BooleanReduction(Intrinsic.GreaterThanOrEqual(a.AsVector(), b.AsVector()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2<bool> LessThanOrEqual(Vector2<T> a, Vector2<T> b) => BooleanReduction(Intrinsic.LessThanOrEqual(a.AsVector(), b.AsVector()));

        #endregion
    }
}
