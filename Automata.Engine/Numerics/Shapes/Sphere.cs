using System;
using System.Numerics;

namespace Automata.Engine.Numerics.Shapes
{
    public readonly struct Sphere : IEquatable<Sphere>
    {
        public static readonly Sphere Zero = new Sphere(Vector3.Zero, 0f);

        public readonly Vector3 Center;
        public readonly float Radius;

        public Sphere(Vector3 center, float radius) => (Center, Radius) = (center, radius);

        public bool Equals(Sphere other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);
        public override bool Equals(object? obj) => obj is Sphere other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Center, Radius);

        public static bool operator ==(Sphere left, Sphere right) => left.Equals(right);
        public static bool operator !=(Sphere left, Sphere right) => !left.Equals(right);
    }
}
