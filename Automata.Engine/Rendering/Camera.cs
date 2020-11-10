#region

using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL;

#endregion


namespace Automata.Engine.Rendering
{
    public class Camera : Component
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Layer RenderedLayers { get; set; } = Layer.Mask;
        public Projector Projector { get; set; } = Projector.None;
        public IProjection? Projection { get; set; }
        public UniformBuffer Uniforms { get; set; }
    }
}
