using System;
using System.Numerics;
using Automata.Engine.Numerics.Shapes;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

namespace Automata.Engine.Rendering
{
    public static class Frustum
    {
        public enum Intersect
        {
            Outside,
            Intersect,
            Inside
        }

        public const int NEAR = 0;
        public const int FAR = 1;
        public const int BOTTOM = 2;
        public const int TOP = 3;
        public const int LEFT = 4;
        public const int RIGHT = 5;
        public const int TOTAL_PLANES = 6;
    }

    public readonly ref struct ClipFrustum
    {
        private readonly ReadOnlySpan<Plane> _Planes;

        public ClipFrustum(Span<Plane> planes, Matrix4x4 mvp)
        {
            if (planes.Length != Frustum.TOTAL_PLANES) throw new ArgumentOutOfRangeException(nameof(planes), "Length must be 6.");

            planes[Frustum.NEAR] = new Plane
            (
                mvp.M13 + mvp.M14,
                mvp.M23 + mvp.M24,
                mvp.M33 + mvp.M34,
                mvp.M43 + mvp.M44
            );

            planes[Frustum.FAR] = new Plane
            (
                mvp.M14 - mvp.M13,
                mvp.M24 - mvp.M23,
                mvp.M34 - mvp.M33,
                mvp.M44 - mvp.M43
            );

            planes[Frustum.BOTTOM] = new Plane
            (
                mvp.M12 + mvp.M14,
                mvp.M22 + mvp.M24,
                mvp.M32 + mvp.M34,
                mvp.M42 + mvp.M44
            );

            planes[Frustum.TOP] = new Plane
            (
                mvp.M14 - mvp.M12,
                mvp.M24 - mvp.M22,
                mvp.M34 - mvp.M32,
                mvp.M44 - mvp.M42
            );

            planes[Frustum.LEFT] = new Plane
            (
                mvp.M11 + mvp.M14,
                mvp.M21 + mvp.M24,
                mvp.M31 + mvp.M34,
                mvp.M41 + mvp.M44
            );

            planes[Frustum.RIGHT] = new Plane
            (
                mvp.M14 - mvp.M11,
                mvp.M24 - mvp.M21,
                mvp.M34 - mvp.M31,
                mvp.M44 - mvp.M41
            );

            _Planes = planes;
        }

        public Frustum.Intersect Intersects(Vector3 point)
        {
            foreach (Plane plane in _Planes)
                if (plane.Distance(point) < 0f)
                    return Frustum.Intersect.Outside;

            return Frustum.Intersect.Inside;
        }

        public Frustum.Intersect Intersects(Sphere sphere)
        {
            Frustum.Intersect result = Frustum.Intersect.Inside;

            foreach (Plane plane in _Planes)
            {
                float distance = plane.Distance(sphere.Center);

                if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
                else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;
            }

            return result;
        }

        public Frustum.Intersect Intersects(Cube cube)
        {
            Frustum.Intersect result = Frustum.Intersect.Inside;

            foreach (Plane plane in _Planes)
                if (plane.Distance(cube.GreaterSumVertex(plane.Normal)) < 0f) return Frustum.Intersect.Outside;
                else if (plane.Distance(cube.LesserSumVertex(plane.Normal)) < 0f) result = Frustum.Intersect.Intersect;

            return result;
        }
    }
}
