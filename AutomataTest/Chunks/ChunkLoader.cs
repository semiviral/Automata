#region

using Automata.Components;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkLoader : IComponent
    {
        private const int _RADIUS = 4;

        public int Radius { get; set; } = _RADIUS;
    }
}
