using System.Collections.Generic;
using System.Numerics;
using Automata.Core;

namespace Automata.Rendering
{
    public class DirtyMeshComponent : IComponent
    {
        public List<Vector3> Vertices { get; set; }
        public List<Color64> Colors { get; set; }
        public List<int> Triangles { get; set; }
    }
}
