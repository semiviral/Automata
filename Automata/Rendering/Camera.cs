#region

using System.Numerics;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {

        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Vector4 ProjectionParameters { get; set; } = Vector4.Zero;

        public Vector3 AccumulatedAngles { get; set; } = Vector3.Zero;
    }
}
