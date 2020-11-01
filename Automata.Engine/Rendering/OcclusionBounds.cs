using Automata.Engine.Components;
using Automata.Engine.Numerics.Shapes;

namespace Automata.Engine.Rendering
{
    public class OcclusionBounds : IComponent
    {
        public Sphere Spheric { get; set; }
        public Cube Cubic { get; set; }
    }
}
