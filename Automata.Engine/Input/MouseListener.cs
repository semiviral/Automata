using Automata.Engine.Numerics;

namespace Automata.Engine.Input
{
    public class MouseListener : Component
    {
        public float Sensitivity { get; set; }
        public Vector2<float> AccumulatedAngles { get; set; }
    }
}
