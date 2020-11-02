#region

using System.Numerics;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Rendering
{
    public class Camera : IComponent
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Layer RenderedLayers { get; set; } = Layer.Mask;
        public Projector Projector { get; set; } = Projector.None;
        public IProjection? Projection { get; set; }
    }
}
