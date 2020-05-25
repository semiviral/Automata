#region

using System.Numerics;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        private static readonly Shader _DefaultShader = new Shader();

        public Matrix4x4 View { get; set; }
        public Matrix4x4 Projection { get; set; }

        public Shader Shader { get; set; }
        public Vector3 AccumulatedAngles { get; set; }

        public Camera()
        {
            Shader = _DefaultShader;
            AccumulatedAngles = Vector3.Zero;
        }
    }
}
