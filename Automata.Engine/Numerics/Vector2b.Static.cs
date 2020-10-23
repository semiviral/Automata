// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global


#region

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endregion


namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector2b
    {
        public static bool All(Vector2b a) => a.X && a.Y;
        public static bool Any(Vector2b a) => a.X || a.Y;


        #region Intrinsics

        private static Vector2b EqualsImpl(Vector2b a, Vector2b b)
        {
            if (Sse2.IsSupported) return (Vector2b)Sse2.CompareEqual((Vector128<byte>)a, (Vector128<byte>)b);
            else
            {
                static Vector2b SoftwareFallback(Vector2b a0, Vector2b b0) => new Vector2b(a0.X == b0.X, a0.Y == b0.Y);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector2b NotEqualsImpl(Vector2b a, Vector2b b)
        {
            static Vector2b SoftwareFallback(Vector2b a0, Vector2b b0) => new Vector2b(a0.X != b0.X, a0.Y != b0.Y);

            return SoftwareFallback(a, b);
        }

        #endregion
    }
}
