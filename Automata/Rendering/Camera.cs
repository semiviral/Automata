#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        private static readonly Shader _DefaultShader = new Shader();

        private Matrix4x4 _View = Matrix4x4.Identity;
        private Matrix4x4 _Projection = Matrix4x4.Identity;
        private Matrix4x4 _Model = Matrix4x4.Identity;

        public Shader Shader { get; set; } = _DefaultShader;

        public Matrix4x4 View
        {
            get => _View;
            set
            {
                _View = value;

                Shader?.SetUniform(nameof(View), _View);
            }
        }

        public Matrix4x4 Projection
        {
            get => _Projection;
            set
            {
                _Projection = value;

                Shader?.SetUniform(nameof(Projection), _Projection);
            }
        }

        public Matrix4x4 Model
        {
            get => _Model;
            set
            {
                _Model = value;

                Shader?.SetUniform(nameof(Model), _Model);
            }
        }
    }
}
