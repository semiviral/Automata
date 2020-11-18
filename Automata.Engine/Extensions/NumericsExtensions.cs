using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Extensions
{
    public static class NumericsExtensions
    {
        public static Vector3 RoundBy(this Vector3 a, Vector3 by)
        {
            Vector3 rounded = a / by;
            rounded = new Vector3(MathF.Floor(rounded.X), MathF.Floor(rounded.Y), MathF.Floor(rounded.Z));
            return rounded * by;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector2 a) => a.X + a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector3 a) => a.X + a.Y + a.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector4 a) => a.X + a.Y + a.Z + a.W;
    }
}
