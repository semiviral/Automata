#region

using System.Numerics;
using Automata.Core;

#endregion

namespace Automata.Rendering
{
    public class PendingMeshDataComponent : IComponent
    {
        public Vector3[] Vertices { get; set; }
        public Color64[] Colors { get; set; }
        public uint[] Triangles { get; set; }
    }
}
