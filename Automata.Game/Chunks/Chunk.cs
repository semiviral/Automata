using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; } = GenerationState.AwaitingTerrain;
        public bool InsertionSafeState => State is GenerationState.Finished or GenerationState.AwaitingMesh;
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, false);

        public IEnumerable<Palette<Block>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public bool IsStateLockstep(bool exact)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool StateCompare(GenerationState self, GenerationState other, bool exact) => (exact && (other == self)) || (!exact && (other >= self));

            return Neighbors.All(chunk => chunk is null || StateCompare(State, chunk.State, exact));
        }

        public void RemeshNeighbors()
        {
            foreach (Chunk? chunk in Neighbors)
                if (chunk?.InsertionSafeState is true)
                    chunk.State = GenerationState.AwaitingMesh;
        }
    }
}
