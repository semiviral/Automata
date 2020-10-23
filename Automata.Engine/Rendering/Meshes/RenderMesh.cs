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
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;
        public IMesh Mesh { get; set; }

        public RenderMesh(IMesh mesh) => Mesh = mesh;
    }
}
