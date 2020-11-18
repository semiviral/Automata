using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// ReSharper disable InconsistentNaming

namespace Automata.Engine.Numerics
{
    public readonly partial struct Vector3b
    {
        public static bool All(Vector3b a) => a.X && a.Y && a.Z;
        public static bool Any(Vector3b a) => a.X || a.Y || a.Z;


        #region Instrinsics

        private static Vector3b EqualsImpl(Vector3b a, Vector3b b)
        {
            if (Sse2.IsSupported) return (Vector3b)Sse2.CompareEqual((Vector128<byte>)a, (Vector128<byte>)b);
            else
            {
                static Vector3b SoftwareFallback(Vector3b a0, Vector3b b0) => new Vector3b(a0.X == b0.X, a0.Y == b0.Y, a0.Z == b0.Z);

                return SoftwareFallback(a, b);
            }
        }

        private static Vector3b NotEqualsImpl(Vector3b a, Vector3b b)
        {
            static Vector3b SoftwareFallback(Vector3b a0, Vector3b b0) => new Vector3b(a0.X != b0.X, a0.Y != b0.Y, a0.Z != b0.Z);

            return SoftwareFallback(a, b);
        }

        private static Vector3b OrImpl(Vector3b a, Vector3b b)
        {
            static Vector3b SoftwareFallback(Vector3b a0, Vector3b b0) =>
                new Vector3b((byte)(a0._X | b0._X), (byte)(a0._Y | b0._Y), (byte)(a0._Z | b0._Z));

            if (Sse2.IsSupported) return (Vector3b)Sse2.Or((Vector128<byte>)a, (Vector128<byte>)b);
            else return SoftwareFallback(a, b);
        }

        #endregion
    }
}
