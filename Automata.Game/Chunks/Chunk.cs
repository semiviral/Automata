using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; }
        public Palette<Block>? Blocks { get; set; }
        public int TimesMeshed { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public IEnumerable<Palette<Block>?> NeighborBlocks => Neighbors.Select(chunk => chunk?.Blocks);
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, true);

        public void RemeshNeighborhood(bool remesh)
        {
            if (!remesh) return;

            State = GenerationState.AwaitingMesh;

            foreach (Chunk? neighbor in Neighbors)
                if (neighbor?.State is > GenerationState.AwaitingMesh)
                    neighbor.State = GenerationState.AwaitingMesh;
        }
    }
}
