using System.Numerics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Extensions
{
    public static class NumericsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundBy(this Vector3 a, Vector3 by) => (a / by).Floor() * by;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundBy(this Vector3 a, float by) => (a / by).Floor() * by;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 a) => new Vector3(a.X.FastFloor(), a.Y.FastFloor(), a.Z.FastFloor());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector2 a) => a.X + a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector3 a) => a.X + a.Y + a.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector4 a) => a.X + a.Y + a.Z + a.W;
    }
}
