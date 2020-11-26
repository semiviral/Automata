using System.Numerics;

namespace Automata.Engine.Input
{
    public class MouseListener : Component
    {
        public float Sensitivity { get; set; }
        public Vector2 AccumulatedAngles { get; set; }
    }
}
