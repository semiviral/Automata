using System;
using System.Numerics;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

namespace Automata.Engine.Rendering
{
    public class ConstantFrustum
    {
        public Vector3 LocalForward { get; private set; }
        public Vector3 LocalUp { get; private set; }
        public Vector3 RelativeRight { get; private set; }

        private Plane[] _Planes { get; }

        public ConstantFrustum() => _Planes = new Plane[6];

        public void CalculateVectorFields(Quaternion rotation)
        {
            LocalForward = Vector3.Transform(-Vector3.UnitZ, Quaternion.Inverse(rotation));
            RelativeRight = Vector3.Normalize(Vector3.Cross(LocalForward, Vector3.UnitY));
            LocalUp = Vector3.Normalize(Vector3.Cross(LocalForward, RelativeRight));
        }

        public void CalculatePlanes(Vector3 position, float fov, float aspectRatio, float nearDistance, float farDistance)
        {
            Vector2 nearPlane = CalculateClipPlaneDimensions(fov, aspectRatio, nearDistance);
            Vector2 farPlane = CalculateClipPlaneDimensions(fov, aspectRatio, farDistance);

            Vector3 nearCenter = position + (LocalForward * nearDistance);
            Vector3 farCenter = position + (LocalForward * farDistance);

            Vector3 nearTopLeft = (nearCenter - (LocalUp * nearPlane.Y)) + (RelativeRight * nearPlane.X);
            Vector3 nearTopRight = nearCenter + (LocalUp * nearPlane.Y) + (RelativeRight * nearPlane.X);
            Vector3 nearBottomLeft = nearCenter - (LocalUp * nearPlane.Y) - (RelativeRight * nearPlane.X);
            Vector3 nearBottomRight = (nearCenter + (LocalUp * nearPlane.Y)) - (RelativeRight * nearPlane.X);

            Vector3 farTopLeft = (farCenter - (LocalUp * farPlane.Y)) + (RelativeRight * farPlane.X);
            Vector3 farTopRight = farCenter + (LocalUp * farPlane.Y) + (RelativeRight * farPlane.X);
            Vector3 farBottomLeft = farCenter - (LocalUp * farPlane.Y) - (RelativeRight * farPlane.X);
            Vector3 farBottomRight = (farCenter + (LocalUp * farPlane.Y)) - (RelativeRight * farPlane.X);

            _Planes[Frustum.NEAR] = new Plane(nearCenter, LocalForward);
            _Planes[Frustum.FAR] = new Plane(farCenter, -LocalForward);

            _Planes[Frustum.BOTTOM] = new Plane(farBottomRight, nearCenter - (LocalUp * nearPlane.X), farBottomLeft);
            _Planes[Frustum.TOP] = new Plane(farTopLeft, nearCenter + (LocalUp * farPlane.X), farTopRight);

            _Planes[Frustum.LEFT] = new Plane(farBottomLeft, nearCenter - (RelativeRight * nearPlane.X), farTopLeft);
            _Planes[Frustum.RIGHT] = new Plane(farTopRight, nearCenter + (RelativeRight * nearPlane.X), farBottomRight);
        }

        private static float CalculateClipPlaneHeight(float fov, float distance) => 2f * (float)Math.Tan(fov / 2f) * distance;
        private static float CalculateClipPlaneWidth(float clipPlaneHeight, float aspectRatio) => clipPlaneHeight * aspectRatio;

        public static Vector2 CalculateClipPlaneDimensions(float fov, float aspectRatio, float distance)
        {
            float clipPlaneHeight = CalculateClipPlaneHeight(fov, distance);
            float clipPlaneWidth = CalculateClipPlaneWidth(clipPlaneHeight, aspectRatio);
            return new Vector2(clipPlaneWidth, clipPlaneHeight);
        }
    }
}
