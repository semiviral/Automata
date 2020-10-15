#region

using System;
using Automata.Engine.Components;

#endregion

namespace Automata.Engine.Rendering.Meshes
{
    public class RenderMesh : IComponent
    {
        public Guid MeshID { get; } = Guid.NewGuid();
        public IMesh? Mesh { get; set; }
    }
}
