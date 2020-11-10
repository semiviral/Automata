#region

using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;

#endregion


namespace Automata.Engine.Noise
{
    public static class OpenSimplexSlim
    {
        private const short _FN_INLINE = 256;
        private const int _X_PRIME = 1619;
        private const int _Y_PRIME = 31337;
        private const int _Z_PRIME = 6971;

        public static float GetSimplex(int seed, float frequency, Vector2 coords) => Simplex2D(seed, frequency, coords);
        public static float GetSimplex(int seed, float frequency, Vector3 coords) => Simplex3D(seed, frequency, coords);


        #region Simplex2D

        private static readonly Vector2[] _Grad2D =
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(1, 0)
        };

        private const float _F2 = (float)(1.0 / 2.0);
        private const float _G2 = (float)(1.0 / 4.0);

        [MethodImpl(_FN_INLINE)]
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

        private static float Simplex2D(int seed, float frequency, Vector2 xy)
        {
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

            t = 0.5f - (xy1.X * xy1.X) - (xy1.Y * xy1.Y);

            if (t < 0f) n0 = 0f;
            else
            {
                t *= t;
                n0 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij), xy1);
            }

            t = 0.5f - (xy2.X * xy2.X) - (xy2.Y * xy2.Y);

            if (t < 0f) n1 = 0f;
            else
            {
                t *= t;
                n1 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij + ij1), xy2);
            }

            t = 0.5f - (xy3.X * xy3.X) - (xy3.Y * xy3.Y);

            if (t < 0f) n2 = 0f;
            else
            {
                t *= t;
                n2 = t * t * GradCoord2D(seed, Vector2i.FromVector2(ij + Vector2.One), xy3);
            }

            return 50f * (n0 + n1 + n2);
        }

        #endregion


        #region Simplex3D

        private const float _F3 = (float)(1.0 / 3.0);
        private const float _G3 = (float)(1.0 / 6.0);
        private const float _G33 = (_G3 * 3) - 1;

        private static readonly Vector3[] _Grad3D =
        {
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(1, 0, 1),
            new Vector3(-1, 0, 1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, -1),
            new Vector3(0, 1, 1),
            new Vector3(0, -1, 1),
            new Vector3(0, 1, -1),
            new Vector3(0, -1, -1),
            new Vector3(1, 1, 0),
            new Vector3(0, -1, 1),
            new Vector3(-1, 1, 0),
            new Vector3(0, -1, -1)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastFloor(float f) => f >= 0 ? (int)f : (int)f - 1;

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

        private static float Simplex3D(int seed, float frequency, Vector3 xyz)
        {
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

            t = 0.6f - (xyz0.X * xyz0.X) - (xyz0.Y * xyz0.Y) - (xyz0.Z * xyz0.Z);

            if (t < 0f) n0 = 0f;
            else
            {
                t *= t;
                n0 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk), xyz0);
            }

            t = 0.6f - (xyz1.X * xyz1.X) - (xyz1.Y * xyz1.Y) - (xyz1.Z * xyz1.Z);

            if (t < 0f) n1 = 0f;
            else
            {
                t *= t;
                n1 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + ijk1), xyz1);
            }

            t = 0.6f - (xyz2.X * xyz2.X) - (xyz2.Y * xyz2.Y) - (xyz2.Z * xyz2.Z);

            if (t < 0f) n2 = 0f;
            else
            {
                t *= t;
                n2 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + ijk2), xyz2);
            }

            t = 0.6f - (xyz3.X * xyz3.X) - (xyz3.Y * xyz3.Y) - (xyz3.Z * xyz3.Z);

            if (t < 0f) n3 = 0f;
            else
            {
                t *= t;
                n3 = t * t * GradCoord3D(seed, Vector3i.FromVector3(ijk + Vector3.One), xyz3);
            }

            return 32f * (n0 + n1 + n2 + n3);
        }

        #endregion
    }
}
