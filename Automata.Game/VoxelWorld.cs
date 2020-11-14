using Automata.Engine;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Memory;
using Automata.Game.Chunks;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        public ChunkMap Chunks { get; }
        public BufferAllocator Allocator { get; }

        public VoxelWorld(bool active) : base(active)
        {
            const uint one_kb = 1000u;
            const uint one_mb = 1000u * one_kb;
            const uint one_gb = 1000u * one_mb;

            Chunks = new ChunkMap();
            Allocator = new BufferAllocator(GLAPI.Instance.GL, one_gb);
        }
    }
}
