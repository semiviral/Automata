using System.Numerics;
using System.Runtime.CompilerServices;

namespace Automata.Engine.Numerics
{
    public readonly struct Plane
    {
        public readonly Vector3 Normal;
        public readonly float D;

        public Plane(Vector3 normal, float d) => (Normal, D) = (normal, d);

        public Plane(float a, float b, float c, float d)
        {
            Normal = new Vector3(a, b, c);
            float length = Normal.Length();
            Normal = Vector3.Normalize(Normal);
            D = d / length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(Vector3 point) => D + Vector3.Dot(Normal, point);
    }
}
