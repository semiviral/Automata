using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Numerics.Shapes
{
    public readonly struct Plane : IEquatable<Plane>
    {
        public readonly Vector3 Normal;
        public readonly Vector3 Point;
        public readonly float D;

        public Plane(Vector3 normal, float d) => (Normal, Point, D) = (normal, Vector3.Zero, d);

        public Plane(float a, float b, float c, float d)
        {
            Point = Vector3.Zero;
            Normal = new Vector3(a, b, c);
            float length = Normal.Length();
            Normal = Vector3.Normalize(Normal);
            D = d / length;
        }

        public Plane(Vector3 normal, Vector3 point) => (Normal, Point, D) = (Vector3.Normalize(normal), point, -Vector3.Dot(normal, point));

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Normal = Vector3.Normalize((a - b) * (c - b));
            Point = b;
            D = -Vector3.Dot(Normal, Point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(Vector3 point) => D + Vector3.Dot(Normal, point);

        public bool Equals(Plane other) => Normal.Equals(other.Normal) && D.Equals(other.D);
        public override bool Equals(object? obj) => obj is Plane other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Normal, Point, D);

        public static bool operator ==(Plane left, Plane right) => left.Equals(right);
        public static bool operator !=(Plane left, Plane right) => !left.Equals(right);
    }
}
