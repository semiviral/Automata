using System;
using System.Numerics;

namespace Automata.Engine.Rendering
{
    public class Frustum
    {
        public Vector2 NearPlaneDimensions { get; private set; }
        public Vector2 FarPlaneDimensions { get; private set; }

        public Vector3 NearTopLeft { get; private set; }
        public Vector3 NearTopRight { get; private set; }
        public Vector3 NearBottomLeft { get; private set; }
        public Vector3 NearBottomRight { get; private set; }
        public Vector3 FarTopLeft { get; private set; }
        public Vector3 FarTopRight { get; private set; }
        public Vector3 FarBottomLeft { get; private set; }
        public Vector3 FarBottomRight { get; private set; }

        private static float CalculateClipPlaneHeight(float fov, float distance) => 2f * (float)Math.Tan(fov / 2f) * distance;
        private static float CalculateClipPlaneWidth(float clipPlaneHeight, float aspectRatio) => clipPlaneHeight * aspectRatio;

        public static Vector2 CalculateClipPlaneDimensions(float fov, float aspectRatio, float distance)
        {
            float clipPlaneHeight = CalculateClipPlaneHeight(fov, distance);
            float clipPlaneWidth = CalculateClipPlaneWidth(clipPlaneHeight, aspectRatio);
            return new Vector2(clipPlaneWidth, clipPlaneHeight);
        }

        public void CalculateFrustum(Vector3 origin, Quaternion rotation, float fov, float aspectRatio, float nearDistance, float farDistance)
        {
            FarPlaneDimensions = CalculateClipPlaneDimensions(fov, aspectRatio, farDistance);
            NearPlaneDimensions = CalculateClipPlaneDimensions(fov, aspectRatio, nearDistance);

            Vector3 right = Vector3.Transform(Vector3.UnitX, rotation);
            Vector3 up = Vector3.Transform(Vector3.UnitY, rotation);
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, rotation);

            Vector3 nearClip = origin + (forward * nearDistance);
            Vector3 farClip = origin + (forward * farDistance);

            Vector3 nearWidth = new Vector3(NearPlaneDimensions.X);
            Vector3 nearHeight = new Vector3(NearPlaneDimensions.Y);
            Vector3 farWidth = new Vector3(FarPlaneDimensions.X);
            Vector3 farHeight = new Vector3(FarPlaneDimensions.Y);

            Matrix4x4.Identity.

            NearTopLeft = nearClip + (up * (NearPlaneDimensions.Y / 2f)) - (right * (NearPlaneDimensions.Y / 2f));
            NearTopRight = NearTopLeft + nearWidth;
            NearBottomLeft = NearTopLeft - nearHeight;
            NearBottomRight = NearTopLeft + nearWidth - nearHeight;

            FarTopLeft = farClip + (up * (FarPlaneDimensions.X / 2f)) - (right * (FarPlaneDimensions.Y / 2f));
            FarTopRight = farClip + farWidth;
            FarBottomLeft = farClip - farHeight;
            FarBottomRight = farClip + farWidth - farHeight;

            Vector3 tmp = Vector3.Normalize(nearClip + right * (NearPlaneDimensions.X / 2f) - origin);
            Vector3 normalRight = Vector3.Cross(up, tmp);
        }
    }
}
