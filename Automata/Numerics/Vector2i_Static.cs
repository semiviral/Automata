#region

using System;
using System.Runtime.Intrinsics;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace Automata.Numerics
{
    public readonly partial struct Vector2i
    {
        public static Vector2i Select(Vector2i a, Vector2b selector) =>
            (Vector2i)VectorConstants.BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)selector);

        public static Vector2i SelectByGreaterThan(Vector2i select, Vector2i a, Vector2i b) =>
            (Vector2i)VectorConstants.BitwiseAndImpl((Vector128<int>)select, VectorConstants.GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static Vector2i SelectByLessThan(Vector2i select, Vector2i a, Vector2i b) =>
            (Vector2i)VectorConstants.BitwiseAndImpl((Vector128<int>)select, VectorConstants.LessThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static int Sum(Vector2i a) => a.X + a.Y;

        public static Vector2i Project3D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            int zQuotient = Math.DivRem(xQuotient, bounds, out int z);
            int y = zQuotient % bounds;
            return new Vector2i(x, y, z);
        }

        public static int Project1D(Vector2i a, int size) => a.X + (size * a.Y);
    }
}
