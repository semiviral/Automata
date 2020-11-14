using Automata.Engine;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Memory;
using Automata.Game.Chunks;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        public ChunkMap Chunks { get; }

        public VoxelWorld(bool active) : base(active) => Chunks = new ChunkMap();
    }
}
