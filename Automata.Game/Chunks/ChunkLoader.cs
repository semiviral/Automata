#region

using Automata.Engine.Components;
using Automata.Engine.Numerics;

#endregion

namespace Automata.Game.Chunks
{
    public class ChunkLoader : IComponent
    {
        private const int _RADIUS = 4;

        public int Radius { get; set; } = _RADIUS;
        public Vector3i Origin { get; set; } = new Vector3i(int.MaxValue);
    }
}
