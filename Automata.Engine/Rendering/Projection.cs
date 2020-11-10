using System.Numerics;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;

namespace Automata.Engine.Rendering
{
    public enum Projector
    {
        None,
        Perspective,
        Orthographic
    }

    public interface IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }
    }

    public readonly struct PerspectiveProjection : IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }

        public PerspectiveProjection(float fieldOfView, float aspectRatio, float nearClippingPlane, float farClippingPlane) => (Matrix, Parameters) = (
            Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView.ToRadians(), aspectRatio, nearClippingPlane, farClippingPlane),
            new Vector4(fieldOfView, aspectRatio, nearClippingPlane, farClippingPlane));
    }

    public readonly struct OrthographicProjection : IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }

        public OrthographicProjection(Vector2i size, float nearClippingPlane, float farClippingPlane)
            : this(size.X, size.Y, nearClippingPlane, farClippingPlane) { }

        public OrthographicProjection(float width, float height, float nearClippingPlane, float farClippingPlane) =>
            (Matrix, Parameters) = (Matrix4x4.CreateOrthographic(width, height, nearClippingPlane, farClippingPlane),
                new Vector4(width, height, nearClippingPlane, farClippingPlane));
    }
}
