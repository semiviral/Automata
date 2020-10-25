using System;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    }

    public readonly ref struct ClipFrustum
    {
        public const int PLANES_SPAN_LENGTH = 6;

        private readonly ReadOnlySpan<Plane> _Planes;

        public ClipFrustum(Span<Plane> planes, Matrix4x4 mvp)
        {
            if (planes.Length != PLANES_SPAN_LENGTH) throw new ArgumentOutOfRangeException(nameof(planes), "Length must be 6.");

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

        public Frustum.Intersect PointWithin(Vector3 point) =>
            (_Planes[Frustum.NEAR].Distance(point) < 0f)
            || (_Planes[Frustum.FAR].Distance(point) < 0f)
            || (_Planes[Frustum.BOTTOM].Distance(point) < 0f)
            || (_Planes[Frustum.TOP].Distance(point) < 0f)
            || (_Planes[Frustum.LEFT].Distance(point) < 0f)
            || (_Planes[Frustum.RIGHT].Distance(point) < 0f)
                ? Frustum.Intersect.Outside
                : Frustum.Intersect.Inside;

        public Frustum.Intersect SphereWithin(Sphere sphere)
        {
            Frustum.Intersect result = Frustum.Intersect.Inside;

            float distance = _Planes[Frustum.NEAR].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            distance = _Planes[Frustum.FAR].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            distance = _Planes[Frustum.BOTTOM].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            distance = _Planes[Frustum.TOP].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            distance = _Planes[Frustum.LEFT].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            distance = _Planes[Frustum.RIGHT].Distance(sphere.Center);

            if (distance < -sphere.Radius) return Frustum.Intersect.Outside;
            else if (distance < sphere.Radius) result = Frustum.Intersect.Intersect;

            return result;
        }

        public Frustum.Intersect BoxWithin(Cube cube)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool BoxOutsidePlane(Plane plane, Cube box) => plane.Distance(box.GreaterSumVertex(plane.Normal)) < 0f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool BoxIntersectPlane(Plane plane, Cube box) => plane.Distance(box.LesserSumVertex(plane.Normal)) < 0f;

            Frustum.Intersect result = Frustum.Intersect.Inside;

            Plane plane = _Planes[Frustum.NEAR];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            plane = _Planes[Frustum.FAR];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            plane = _Planes[Frustum.BOTTOM];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            plane = _Planes[Frustum.TOP];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            plane = _Planes[Frustum.LEFT];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            plane = _Planes[Frustum.RIGHT];

            if (BoxOutsidePlane(plane, cube)) return Frustum.Intersect.Outside;
            else if (BoxIntersectPlane(plane, cube)) result = Frustum.Intersect.Intersect;

            return result;
        }

        public Frustum.Intersect BoxWithin(Cube cube)
        {
            Frustum.Intersect result = Frustum.Intersect.Inside;

            foreach (Plane plane in _Planes)
            {
                if (plane.Distance(cube.GreaterSumVertex(plane.Normal)) < 0f) return Frustum.Intersect.Outside;
                else if (plane.Distance(cube.LesserSumVertex(plane.Normal)) < 0f) result = Frustum.Intersect.Intersect;
            }

            return result;
        }

        // public void Recalculate(Matrix4x4 mvp) { }

        // public Vector2 NearPlaneDimensions { get; private set; }
        // public Vector2 FarPlaneDimensions { get; private set; }
        //
        // public Vector3 NearTopLeft { get; private set; }
        // public Vector3 NearTopRight { get; private set; }
        // public Vector3 NearBottomLeft { get; private set; }
        // public Vector3 NearBottomRight { get; private set; }
        // public Vector3 FarTopLeft { get; private set; }
        // public Vector3 FarTopRight { get; private set; }
        // public Vector3 FarBottomLeft { get; private set; }
        // public Vector3 FarBottomRight { get; private set; }
        //
        // public
        //
        // private static float CalculateClipPlaneHeight(float fov, float distance) => 2f * (float)Math.Tan(fov / 2f) * distance;
        // private static float CalculateClipPlaneWidth(float clipPlaneHeight, float aspectRatio) => clipPlaneHeight * aspectRatio;
        //
        // public static Vector2 CalculateClipPlaneDimensions(float fov, float aspectRatio, float distance)
        // {
        //     float clipPlaneHeight = CalculateClipPlaneHeight(fov, distance);
        //     float clipPlaneWidth = CalculateClipPlaneWidth(clipPlaneHeight, aspectRatio);
        //     return new Vector2(clipPlaneWidth, clipPlaneHeight);
        // }
        //
        // public void CalculateFrustum(Vector3 origin, Quaternion rotation, float fov, float aspectRatio, float nearDistance, float farDistance)
        // {
        //     FarPlaneDimensions = CalculateClipPlaneDimensions(fov, aspectRatio, farDistance);
        //     NearPlaneDimensions = CalculateClipPlaneDimensions(fov, aspectRatio, nearDistance);
        //
        //     Vector3 right = Vector3.Transform(Vector3.UnitX, rotation);
        //     Vector3 up = Vector3.Transform(Vector3.UnitY, rotation);
        //     Vector3 forward = Vector3.Transform(Vector3.UnitZ, rotation);
        //
        //     Vector3 nearClip = origin + (forward * nearDistance);
        //     Vector3 farClip = origin + (forward * farDistance);
        //
        //     Vector3 nearWidth = new Vector3(NearPlaneDimensions.X);
        //     Vector3 nearHeight = new Vector3(NearPlaneDimensions.Y);
        //     Vector3 farWidth = new Vector3(FarPlaneDimensions.X);
        //     Vector3 farHeight = new Vector3(FarPlaneDimensions.Y);
        //
        //     NearTopLeft = nearClip + (up * (NearPlaneDimensions.Y / 2f)) - (right * (NearPlaneDimensions.Y / 2f));
        //     NearTopRight = NearTopLeft + nearWidth;
        //     NearBottomLeft = NearTopLeft - nearHeight;
        //     NearBottomRight = NearTopLeft + nearWidth - nearHeight;
        //
        //     FarTopLeft = farClip + (up * (FarPlaneDimensions.X / 2f)) - (right * (FarPlaneDimensions.Y / 2f));
        //     FarTopRight = farClip + farWidth;
        //     FarBottomLeft = farClip - farHeight;
        //     FarBottomRight = farClip + farWidth - farHeight;
        //
        //     Vector3 tmp = Vector3.Normalize(nearClip + right * (NearPlaneDimensions.X / 2f) - origin);
        //     Vector3 normalRight = Vector3.Cross(up, tmp);
        // }
    }
}
