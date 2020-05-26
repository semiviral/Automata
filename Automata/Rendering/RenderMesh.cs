#region

using System;

#endregion

namespace Automata.Rendering
{
    public class RenderMesh : IComponent
    {
        public Guid MeshID { get; } = Guid.NewGuid();
        public IMesh? Mesh { get; set; }
    }
}
