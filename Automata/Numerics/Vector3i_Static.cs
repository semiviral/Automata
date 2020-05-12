#region

using System;
using System.Numerics;
using System.Runtime.Intrinsics;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast

#endregion


namespace Automata.Numerics
{
    public readonly partial struct Vector3i
    {
        public static Vector3i Select(Vector3i a, Vector3b selector) =>
            (Vector3i)VectorConstants.BitwiseAndImpl((Vector128<int>)a, (Vector128<int>)selector);

        public static Vector3i SelectByGreaterThan(Vector3i select, Vector3i a, Vector3i b) =>
            (Vector3i)VectorConstants.BitwiseAndImpl((Vector128<int>)select, VectorConstants.GreaterThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static Vector3i SelectByLessThan(Vector3i select, Vector3i a, Vector3i b) =>
            (Vector3i)VectorConstants.BitwiseAndImpl((Vector128<int>)select, VectorConstants.LessThanImpl((Vector128<int>)a, (Vector128<int>)b));

        public static int Sum(Vector3i a) => a.X + a.Y + a.Z;

        public static Vector3i Project3D(int index, int bounds)
        {
            int xQuotient = Math.DivRem(index, bounds, out int x);
            int zQuotient = Math.DivRem(xQuotient, bounds, out int z);
            int y = zQuotient % bounds;
            return new Vector3i(x, y, z);
        }

        public static int Project1D(Vector3i a, int size) => a.X + (size * (a.Z + (size * a.Y)));

        public static Vector3i FromVector3(Vector3 a) => new Vector3i((int)a.X, (int)a.Y, (int)a.Z);
    }
}
