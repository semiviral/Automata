using System;
using System.Numerics;

namespace Automata.Engine.Rendering
{
    public interface IProjection
    {
        public Matrix4x4 Matrix { get; }
        public Vector4 Parameters { get; }

        public static IProjection CreateFromProjector(Projector projector) =>
            projector switch
            {
                // todo some succint way to pass the projection parameters in
                Projector.Perspective => new PerspectiveProjection(90f, AutomataWindow.Instance.AspectRatio, 0.1f, 1000f),
                Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                _ => throw new ArgumentOutOfRangeException(nameof(projector))
            };
    }
}
