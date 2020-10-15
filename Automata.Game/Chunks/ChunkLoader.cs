#region

using Automata.Engine.Components;

#endregion

namespace Automata.Game.Chunks
{
    public class ChunkLoader : IComponent
    {
        private const int _RADIUS = 4;

        public int Radius { get; set; } = _RADIUS;
    }
}
