using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Worlds;
using Automata.Game.Chunks;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        public ChunkMap Chunks { get; }
        public ApartmentBuffer ChunkAllocator { get; }

        public VoxelWorld(bool active) : base(active) => Chunks = new ChunkMap();
    }
}
