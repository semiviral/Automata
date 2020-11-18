using Automata.Engine.Components;
using Automata.Engine.Numerics.Shapes;

namespace Automata.Engine.Rendering
{
    public class OcclusionBounds : Component
    {
        /// <summary>
        ///     Spheric bounds, used for a quick and inaccurate test of bounds intersection.
        /// </summary>
        public Sphere Spheric { get; set; }

        /// <summary>
        ///     Cubic bounds, used for a more accurate test of bounds intersection.
        /// </summary>
        public Cube Cubic { get; set; }
    }
}
