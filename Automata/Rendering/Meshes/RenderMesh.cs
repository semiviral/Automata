#region

using System;
using Automata.Components;

#endregion

namespace Automata.Rendering.Meshes
{
    public class RenderMesh : IComponent
    {
        public Guid MeshID { get; } = Guid.NewGuid();
        public IMesh? Mesh { get; set; }
    }
}
