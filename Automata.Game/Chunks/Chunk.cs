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
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, false);
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


        #region IDisposable

        public void RegionDispose()
        {
            State = GenerationState.Inactive;
            Modifications.Dispose();
            Blocks?.Dispose();
            Array.Clear(Neighbors, 0, Neighbors.Length);
        }

        #endregion
    }
}
