using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; }
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; private set; } = new Chunk?[6];
        public ConcurrentChannel<ChunkModification> Modifications { get; private set; } = new ConcurrentChannel<ChunkModification>(true, true);
        public int TimesMeshed { get; set; }

        public IEnumerable<Palette<Block>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public void RemeshNeighborhood(bool remesh)
        {
            if (!remesh)
            {
                return;
            }

            State = GenerationState.AwaitingMesh;

            foreach (Chunk? neighbor in Neighbors)
            {
                if (neighbor?.State is > GenerationState.AwaitingMesh)
                {
                    neighbor.State = GenerationState.AwaitingMesh;
                }
            }
        }

        public void SafeDispose()
        {
            State = GenerationState.Inactive;
            Blocks?.Dispose();
            Neighbors = null!;
            Modifications = null!;
            TimesMeshed = 0;

            GC.SuppressFinalize(this);
        }
    }
}
