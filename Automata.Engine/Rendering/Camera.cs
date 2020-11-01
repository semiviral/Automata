#region

using System.Numerics;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Rendering
{
    public class Camera : IComponent
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Vector4 ProjectionParameters { get; set; } = Vector4.Zero;
        public Layer RenderedLayers { get; set; } = Layer.Mask;

        public void CalculateProjection(Vector4 projectionParameters) =>
            CalculateProjection(projectionParameters.X, projectionParameters.Y, projectionParameters.Z, projectionParameters.W);

        public void CalculateProjection(float fieldOfView, float aspectRatio, float nearClippingPlane, float farClippingPlane) =>
            (Projection, ProjectionParameters) =
            (Matrix4x4.CreatePerspectiveFieldOfView(AutomataMath.ToRadians(fieldOfView), aspectRatio, nearClippingPlane, farClippingPlane),
                new Vector4(fieldOfView, aspectRatio, nearClippingPlane, farClippingPlane));
    }
}
