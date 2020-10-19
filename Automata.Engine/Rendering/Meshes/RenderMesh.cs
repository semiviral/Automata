#region

using System;
using System.Numerics;
using Automata.Engine.Components;

#endregion

namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : IComponent
    {
        public Guid MeshID { get; } = Guid.NewGuid();
        public IMesh? Mesh { get; set; }
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;
    }
}
