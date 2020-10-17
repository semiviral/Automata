#region

using System.Numerics;
using Automata.Engine.Components;

#endregion

namespace Automata.Engine.Rendering
{
    public class Camera : IComponent
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Vector4 ProjectionParameters { get; set; } = Vector4.Zero;
    }
}
