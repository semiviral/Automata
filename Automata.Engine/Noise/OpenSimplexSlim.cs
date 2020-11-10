#region

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;

#endregion


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

        private const float _F2 = 1.0f / 2.0f;
        private const float _G2 = 1.0f / 4.0f;

        private static float Simplex2D(int seed, float frequency, Vector2 xy)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float ComponentSubtract(Vector2 a) => a.X - a.Y;

            xy *= frequency;

            float t = xy.Sum() * _F2;
            Vector2 ij = new Vector2(FastFloor(xy.X + t), FastFloor(xy.Y + t));

            t = ij.Sum() * _G2;
            Vector2 xy0 = ij - new Vector2(t);
            Vector2 xy1 = xy - xy0;
            Vector2 ij1 = xy1.X > xy1.Y ? new Vector2(1, 0) : new Vector2(0, 1);
            Vector2 xy2 = (xy1 - ij1) + new Vector2(_G2);
            Vector2 xy3 = (xy1 - Vector2.One) + new Vector2(_F2);

            float n0, n1, n2;

            t = 0.5f - ComponentSubtract(xy1 * xy1);

            if (t < 0f) n0 = 0f;
            else
            {
                t *= t;
                n0 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij), xy1);
            }

            t = 0.5f - ComponentSubtract(xy2 * xy2);

            if (t < 0f) n1 = 0f;
            else
            {
                t *= t;
                n1 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij + ij1), xy2);
            }

            t = 0.5f - ComponentSubtract(xy3 * xy3);

            if (t < 0f) n2 = 0f;
            else
            {
                t *= t;
                n2 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij + Vector2.One), xy3);
            }

            return 50f * (n0 + n1 + n2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GradCoord2D(int seed, Vector2i xy, Vector2 xy0)
        {
            int hash = seed;
            hash ^= _X_PRIME * xy.X;
            hash ^= _Y_PRIME * xy.Y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Vector2 g = _Grad2D[hash & 7];

            return (xy0.X * g.X) + (xy0.Y * g.Y);
        }

        #endregion


        #region Simplex3D

        private const float _F3 = 1.0f / 3.0f;
        private const float _G3 = 1.0f / 6.0f;
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
            static float ComponentSubtract(Vector3 a) => a.X - a.Y - a.Z;

            xyz *= frequency;

            float t = xyz.Sum() * _F3;
            Vector3 ijk = new Vector3(FastFloor(xyz.X + t), FastFloor(xyz.Y + t), FastFloor(xyz.Z + t));

            t = ijk.Sum() * _G3;
            Vector3 xyz0 = xyz - (ijk - new Vector3(t));
            Vector3 ijk1, ijk2;

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

            Vector3 xyz1 = (xyz0 - ijk1) + new Vector3(_G3);
            Vector3 xyz2 = (xyz0 - ijk2) + new Vector3(_F3);
            Vector3 xyz3 = xyz0 + new Vector3(_G33);

            float n0, n1, n2, n3;

            t = 0.6f - ComponentSubtract(xyz0 * xyz0);

            if (t < 0f) n0 = 0f;
            else
            {
                t *= t;
                n0 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk), xyz0);
            }

            t = 0.6f - ComponentSubtract(xyz1 * xyz1);

            if (t < 0f) n1 = 0f;
            else
            {
                t *= t;
                n1 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + ijk1), xyz1);
            }

            t = 0.6f - ComponentSubtract(xyz2 * xyz2);

            if (t < 0f) n2 = 0f;
            else
            {
                t *= t;
                n2 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + ijk2), xyz2);
            }

            t = 0.6f - ComponentSubtract(xyz3 * xyz3);

            if (t < 0f) n3 = 0f;
            else
            {
                t *= t;
                n3 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + Vector3.One), xyz3);
            }

            return 32f * (n0 + n1 + n2 + n3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GradCoord3D(int seed, Vector3i xyz, Vector3 xyz0)
        {
            int hash = seed;
            hash ^= _X_PRIME * xyz.X;
            hash ^= _Y_PRIME * xyz.Y;
            hash ^= _Z_PRIME * xyz.Z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Vector3 g = _Grad3D[hash & 15];

            return (xyz0.X * g.X) + (xyz0.Y * g.Y) + (xyz0.Z * g.Z);
        }

        #endregion
    }
}
