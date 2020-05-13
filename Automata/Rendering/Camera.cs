using System.Numerics;
using Automata.Core.Components;

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;
    }
}
