using System.Numerics;

namespace Automata.Engine.Numerics
{
    public readonly struct BoundingBox
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Extents;

        public BoundingBox(Vector3 origin, Vector3 extents) => (Origin, Extents) = (origin, extents);

        public Vector3 GreaterSumVertex(Vector3 a)
        {
            Vector3 result = Origin;
            if (a.X > 0f) result.X += Extents.X;
            if (a.Y > 0f) result.Y += Extents.Y;
            if (a.Z > 0f) result.Z += Extents.Z;

            return result;
        }

        public Vector3 LesserSumVertex(Vector3 a)
        {
            Vector3 result = Origin;
            if (a.X < 0f) result.X += Extents.X;
            if (a.Y < 0f) result.Y += Extents.Y;
            if (a.Z < 0f) result.Z += Extents.Z;

            return result;
        }
    }
}
