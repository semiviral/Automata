using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Buffers;

namespace Automata.Engine.Rendering
{
    public class Camera : Component, IDisposable
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Layer RenderedLayers { get; set; } = Layer.All;
        public Projector Projector { get; set; }
        public IProjection? Projection { get; set; }
        public UniformBufferObject? Uniforms { get; set; }


        #region IDisposable

        public void Dispose()
        {
            Uniforms?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
