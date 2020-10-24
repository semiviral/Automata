using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;
using Plane = Automata.Engine.Numerics.Plane;

namespace Automata.Engine.Rendering
{
    public readonly ref struct Frustum
    {
        private const int _NEAR = 0;
        private const int _FAR = 1;
        private const int _BOTTOM = 2;
        private const int _TOP = 3;
        private const int _LEFT = 4;
        private const int _RIGHT = 5;

        public const int PLANES_SPAN_LENGTH = 6;

        public enum Boundary
        {
            Outside,
            Intersect,
            Inside
        }

        private readonly ReadOnlySpan<Plane> _Planes;

        public Frustum(Span<Plane> planes, Matrix4x4 mvp)
        {
            if (planes.Length != PLANES_SPAN_LENGTH) throw new ArgumentOutOfRangeException(nameof(planes), "Length must be 6.");

            planes[_NEAR] = new Plane
            (
                mvp.GetValue(2, 0) + mvp.GetValue(3, 0),
                mvp.GetValue(2, 1) + mvp.GetValue(3, 1),
                mvp.GetValue(2, 2) + mvp.GetValue(3, 2),
                mvp.GetValue(2, 3) + mvp.GetValue(3, 3)
            );

            planes[_FAR] = new Plane
            (
                -mvp.GetValue(2, 0) + mvp.GetValue(3, 0),
                -mvp.GetValue(2, 1) + mvp.GetValue(3, 1),
                -mvp.GetValue(2, 2) + mvp.GetValue(3, 2),
                -mvp.GetValue(2, 3) + mvp.GetValue(3, 3)
            );

            planes[_BOTTOM] = new Plane
            (
                mvp.GetValue(1, 0) + mvp.GetValue(3, 0),
                mvp.GetValue(1, 1) + mvp.GetValue(3, 1),
                mvp.GetValue(1, 2) + mvp.GetValue(3, 2),
                mvp.GetValue(1, 3) + mvp.GetValue(3, 3)
            );

            planes[_TOP] = new Plane
            (
                -mvp.GetValue(1, 0) + mvp.GetValue(3, 0),
                -mvp.GetValue(1, 1) + mvp.GetValue(3, 1),
                -mvp.GetValue(1, 2) + mvp.GetValue(3, 2),
                -mvp.GetValue(1, 3) + mvp.GetValue(3, 3)
            );

            planes[_LEFT] = new Plane
            (
                mvp.GetValue(0, 0) + mvp.GetValue(3, 0),
                mvp.GetValue(0, 1) + mvp.GetValue(3, 1),
                mvp.GetValue(0, 2) + mvp.GetValue(3, 2),
                mvp.GetValue(0, 3) + mvp.GetValue(3, 3)
            );

            planes[_RIGHT] = new Plane
            (
                -mvp.GetValue(0, 0) + mvp.GetValue(3, 0),
                -mvp.GetValue(0, 1) + mvp.GetValue(3, 1),
                -mvp.GetValue(0, 2) + mvp.GetValue(3, 2),
                -mvp.GetValue(0, 3) + mvp.GetValue(3, 3)
            );

            _Planes = planes;
        }

        public Boundary PointWithin(Vector3 point) =>
            (_Planes[_NEAR].Distance(point) < 0f)
            || (_Planes[_FAR].Distance(point) < 0f)
            || (_Planes[_BOTTOM].Distance(point) < 0f)
            || (_Planes[_TOP].Distance(point) < 0f)
            || (_Planes[_LEFT].Distance(point) < 0f)
            || (_Planes[_RIGHT].Distance(point) < 0f)
                ? Boundary.Outside
                : Boundary.Inside;

        public Boundary BoxWithin(BoundingBox box)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool BoxOutsidePlane(Plane plane, BoundingBox box) => plane.Distance(box.GreaterSumVertex(plane.Normal)) < 0f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool BoxIntersectPlane(Plane plane, BoundingBox box) => plane.Distance(box.GetLesserSumVertex(plane.Normal)) > 0f;

            Plane plane = _Planes[_NEAR];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            plane = _Planes[_FAR];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            plane = _Planes[_BOTTOM];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            plane = _Planes[_TOP];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            plane = _Planes[_LEFT];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            plane = _Planes[_RIGHT];

            if (BoxOutsidePlane(plane, box)) return Boundary.Outside;
            else if (BoxIntersectPlane(plane, box)) return Boundary.Intersect;

            return Boundary.Inside;
        }

        /// <summary>
        ///     Recalculates the view frustum using a (model * view * projection) matrix.
        /// </summary>
        /// <remarks>
        ///     I use 'mp' here as a parameter name to make the code easier to read (or more terse).
        /// </remarks>
        /// <param name="mvp">The model* view * projection matrix.</param>
        public void Recalculate(Matrix4x4 mvp) { }

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
