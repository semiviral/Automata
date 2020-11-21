using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Extensions;

namespace Automata.Engine.Noise
{
    public static class OpenSimplexSlim
    {
        private const int _X_PRIME = 1619;
        private const int _Y_PRIME = 31337;
        private const int _Z_PRIME = 6971;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSimplex(int seed, float frequency, Vector2 coords) => Simplex2D(seed, frequency, coords);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSimplex(int seed, float frequency, Vector3 coords) => Simplex3D(seed, frequency, coords);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastFloor(float f) => f >= 0f ? (int)f : (int)(f - 1f);


        #region Simplex2D

        private static readonly Vector2[] _Grad2D =
        {
            new Vector2(-1f, -1f),
            new Vector2(1f, -1f),
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -1f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 0f)
        };

        private const float _F2 = 1f / 2f;
        private const float _G2 = 1f / 4f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PrecalculateSimplexFields2D(float frequency, Vector2 xy, out float t,
            out Vector2 ij0, out Vector2 ij1,
            out Vector2 xy1, out Vector2 xy2, out Vector2 xy3)
        {
            xy *= frequency;

            t = xy.Sum() * _F2;
            ij0 = new Vector2(FastFloor(xy.X + t), FastFloor(xy.Y + t));

            t = ij0.Sum() * _G2;
            Vector2 xy0 = ij0 - new Vector2(t);
            xy1 = xy - xy0;
            ij1 = xy1.X > xy1.Y ? new Vector2(1, 0) : new Vector2(0, 1);
            xy2 = (xy1 - ij1) + new Vector2(_G2);
            xy3 = (xy1 - Vector2.One) + new Vector2(_F2);
        }

        private static float Simplex2D(int seed, float frequency, Vector2 xy)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float CalculateTImpl(Vector2 a) => 0.5f - a.X - a.Y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float GradCoord2DImpl(int seed, Vector2 hashVector, Vector2 gradMultiplier)
            {
                int hash = seed;
                hash ^= _X_PRIME * (int)hashVector.X;
                hash ^= _Y_PRIME * (int)hashVector.Y;

                hash = hash * hash * hash * 60493;
                hash = (hash >> 13) ^ hash;

                return (gradMultiplier * _Grad2D[hash & 7]).Sum();
            }

            PrecalculateSimplexFields2D(frequency, xy, out float t,
                out Vector2 ij0, out Vector2 ij1,
                out Vector2 xy1, out Vector2 xy2, out Vector2 xy3);

            Vector3 n;
            t = CalculateTImpl(xy1 * xy1);

            if (t < 0f)
            {
                n.X = 0f;
            }
            else
            {
                t *= t;
                n.X = t * t * GradCoord2DImpl(seed, ij0, xy1);
            }

            t = CalculateTImpl(xy2 * xy2);

            if (t < 0f)
            {
                n.Y = 0f;
            }
            else
            {
                t *= t;
                n.Y = t * t * GradCoord2DImpl(seed, ij0 + ij1, xy2);
            }

            t = CalculateTImpl(xy3 * xy3);

            if (t < 0f)
            {
                n.Z = 0f;
            }
            else
            {
                t *= t;
                n.Z = t * t * GradCoord2DImpl(seed, ij0 + Vector2.One, xy3);
            }

            return 50f * n.Sum();
        }

        #endregion


        #region Simplex3D

        private const float _F3 = 1f / 3f;
        private const float _G3 = 1f / 6f;
        private const float _G33 = (_G3 * 3f) - 1f;

        private static readonly Vector3[] _Grad3D =
        {
            new Vector3(1f, 1f, 0f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, -1f, 0f),
            new Vector3(-1f, -1f, 0f),
            new Vector3(1f, 0f, 1f),
            new Vector3(-1f, 0f, 1f),
            new Vector3(1f, 0f, -1f),
            new Vector3(-1f, 0f, -1f),
            new Vector3(0f, 1f, 1f),
            new Vector3(0f, -1f, 1f),
            new Vector3(0f, 1f, -1f),
            new Vector3(0f, -1f, -1f),
            new Vector3(1f, 1f, 0f),
            new Vector3(0f, -1f, 1f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(0f, -1f, -1f)
        };

        private static float Simplex3D(int seed, float frequency, Vector3 xyz)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float CalculateTImpl(Vector3 a) => 0.6f - a.X - a.Y - a.Z;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float GradCoord3DImpl(int seed, Vector3 hashVector, Vector3 gradMultiplier)
            {
                int hash = seed;
                hash ^= _X_PRIME * (int)hashVector.X;
                hash ^= _Y_PRIME * (int)hashVector.Y;
                hash ^= _Z_PRIME * (int)hashVector.Z;

                hash = hash * hash * hash * 60493;
                hash = (hash >> 13) ^ hash;

                return (gradMultiplier * _Grad3D[hash & 15]).Sum();
            }

            PrecalculateSimplexFields3D(frequency, xyz, out float t,
                out Vector3 ijk0, out Vector3 ijk1, out Vector3 ijk2,
                out Vector3 xyz0, out Vector3 xyz1, out Vector3 xyz2, out Vector3 xyz3);

            Vector4 n;
            t = CalculateTImpl(xyz0 * xyz0);

            if (t < 0f)
            {
                n.X = 0f;
            }
            else
            {
                t *= t;
                n.X = t * t * GradCoord3DImpl(seed, ijk0, xyz0);
            }

            t = CalculateTImpl(xyz1 * xyz1);

            if (t < 0f)
            {
                n.Y = 0f;
            }
            else
            {
                t *= t;
                n.Y = t * t * GradCoord3DImpl(seed, ijk0 + ijk1, xyz1);
            }

            t = CalculateTImpl(xyz2 * xyz2);

            if (t < 0f)
            {
                n.Z = 0f;
            }
            else
            {
                t *= t;
                n.Z = t * t * GradCoord3DImpl(seed, ijk0 + ijk2, xyz2);
            }

            t = CalculateTImpl(xyz3 * xyz3);

            if (t < 0f)
            {
                n.W = 0f;
            }
            else
            {
                t *= t;
                n.W = t * t * GradCoord3DImpl(seed, ijk0 + Vector3.One, xyz3);
            }

            return 32f * n.Sum();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PrecalculateSimplexFields3D(float frequency, Vector3 xyz, out float t,
            out Vector3 ijk0, out Vector3 ijk1, out Vector3 ijk2,
            out Vector3 xyz0, out Vector3 xyz1, out Vector3 xyz2, out Vector3 xyz3)
        {
            xyz *= frequency;
            t = xyz.Sum() * _F3;
            ijk0 = new Vector3(FastFloor(xyz.X + t), FastFloor(xyz.Y + t), FastFloor(xyz.Z + t));

            t = ijk0.Sum() * _G3;
            xyz0 = xyz - (ijk0 - new Vector3(t));

            if (xyz0.X >= xyz0.Y)
            {
                if (xyz0.Y >= xyz0.Z)
                {
                    ijk1 = new Vector3(1f, 0f, 0f);
                    ijk2 = new Vector3(1f, 1f, 0f);
                }
                else if (xyz0.X >= xyz0.Z)
                {
                    ijk1 = new Vector3(1f, 0f, 0f);
                    ijk2 = new Vector3(1f, 0f, 1f);
                }
                else // x0 < z0
                {
                    ijk1 = new Vector3(0f, 0f, 1f);
                    ijk2 = new Vector3(1f, 0f, 1f);
                }
            }
            else // x0 < y0
            {
                if (xyz0.Y < xyz0.Z)
                {
                    ijk1 = new Vector3(0f, 0f, 1f);
                    ijk2 = new Vector3(0f, 1f, 1f);
                }
                else if (xyz0.X < xyz0.Z)
                {
                    ijk1 = new Vector3(0f, 1f, 0f);
                    ijk2 = new Vector3(0f, 1f, 1f);
                }
                else // x0 >= z0
                {
                    ijk1 = new Vector3(0f, 1f, 0f);
                    ijk2 = new Vector3(1f, 1f, 0f);
                }
            }

            xyz1 = (xyz0 - ijk1) + new Vector3(_G3);
            xyz2 = (xyz0 - ijk2) + new Vector3(_F3);
            xyz3 = xyz0 + new Vector3(_G33);
        }

        #endregion
    }
}
