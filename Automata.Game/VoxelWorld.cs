using Automata.Engine;
using Automata.Game.Chunks;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        public ChunkMap Chunks { get; }

        public VoxelWorld(bool active) : base(active)
        {
            const uint maximum_vertices = 2048;
            const uint slot_size = (maximum_vertices * 2 * 4) + (((maximum_vertices * 3) / 2) * 4);

            Chunks = new ChunkMap();
        }
    }
}
