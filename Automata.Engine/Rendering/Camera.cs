#region

using System.Numerics;
using System.Transactions;
using Automata.Engine.Components;
using Automata.Engine.Entities;

#endregion

namespace Automata.Engine.Rendering
{
    public class Camera : IComponent
    {
        public const float DEFAULT_FIELD_OF_VIEW = 90f;
        public const float NEAR_CLIPPING_PLANE = 0.1f;
        public const float FAR_CLIPPING_PLANE = 1000f;

        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Vector4 ProjectionParameters { get; set; } = Vector4.Zero;

        public float FieldOfView { get; set; } = DEFAULT_FIELD_OF_VIEW;
        public float NearClippingPlane { get; set; } = NEAR_CLIPPING_PLANE;
        public float FarClippingPlane { get; set; } = FAR_CLIPPING_PLANE;

        public void CalculateProjection(float aspectRatio) => (Projection, ProjectionParameters) =
            (Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(FieldOfView), aspectRatio, NearClippingPlane, FarClippingPlane),
                new Vector4(1f, NearClippingPlane, FarClippingPlane, 1f / FarClippingPlane));
    }
}
