using System.Numerics;
using Automata.Engine.Numerics;

namespace Automata.Engine.Rendering
{
    public readonly struct OrthographicProjection : IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }

        public OrthographicProjection(Vector2<int> size, float nearClippingPlane, float farClippingPlane)
            : this(size.X, size.Y, nearClippingPlane, farClippingPlane) { }

        public OrthographicProjection(float width, float height, float nearClippingPlane, float farClippingPlane) =>
            (Matrix, Parameters) = (Matrix4x4.CreateOrthographic(width, height, nearClippingPlane, farClippingPlane),
                new Vector4(width, height, nearClippingPlane, farClippingPlane));
    }
}
