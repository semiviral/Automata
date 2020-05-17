#region

using System.Numerics;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        private static readonly Shader _DefaultShader = new Shader();

        private Matrix4x4 _View;
        private Matrix4x4 _Projection;

        public Shader Shader { get; set; } = _DefaultShader;

        public Vector3 AccumulatedAngles { get; set; }

        public Matrix4x4 View
        {
            get => _View;
            set
            {
                _View = value;

                Shader.SetUniform(nameof(View), _View);
            }
        }

        public Matrix4x4 Projection
        {
            get => _Projection;
            set
            {
                _Projection = value;

                Shader.SetUniform(nameof(Projection), _Projection);
            }
        }
    }
}
