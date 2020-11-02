using System;
using System.Numerics;

namespace Automata.Engine.Numerics.Shapes
{
    public readonly struct Cube : IEquatable<Cube>
    {
        public static Cube Zero { get; } = new Cube(Vector3.Zero, Vector3.Zero);

        public readonly Vector3 Origin;
        public readonly Vector3 Extents;

        public Cube(Vector3 origin, Vector3 extents) => (Origin, Extents) = (origin, extents);

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

        public bool Equals(Cube other) => Origin.Equals(other.Origin) && Extents.Equals(other.Extents);
        public override bool Equals(object? obj) => obj is Cube other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Origin, Extents);

        public static bool operator ==(Cube left, Cube right) => left.Equals(right);
        public static bool operator !=(Cube left, Cube right) => !left.Equals(right);
    }
}
