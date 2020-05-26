#region

using System.Numerics;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        private static readonly Shader _DefaultShader = new Shader();

        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Vector4 ProjectionParameters { get; set; } = Vector4.Zero;

        public Shader Shader { get; set; } = _DefaultShader;
        public Vector3 AccumulatedAngles { get; set; } = Vector3.Zero;
    }
}
