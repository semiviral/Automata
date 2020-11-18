using System.Numerics;
using Automata.Engine.Extensions;

namespace Automata.Engine.Rendering
{
    public readonly struct PerspectiveProjection : IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }

        public PerspectiveProjection(float fieldOfView, float aspectRatio, float nearClippingPlane, float farClippingPlane) => (Matrix, Parameters) = (
            Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView.ToRadians(), aspectRatio, nearClippingPlane, farClippingPlane),
            new Vector4(fieldOfView, aspectRatio, nearClippingPlane, farClippingPlane));
    }
}
