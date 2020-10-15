using Automata.Components;

namespace AutomataTest.Chunks
{
    public class ChunkLoader : IComponent
    {
        private const int _RADIUS = 4;

        public int Radius { get; } = _RADIUS;
    }
}
