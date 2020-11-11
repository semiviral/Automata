using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; } = GenerationState.Ungenerated;
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];

        public IEnumerable<Palette<Block>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public bool IsStateLockstep(bool exact) => Neighbors.All(chunk => chunk is null || StateCompare(State, chunk.State, exact));
        private static bool StateCompare(GenerationState self, GenerationState other, bool exact) => (exact && (other == self)) || (!exact && (other >= self));
    }

    public enum GenerationState
    {
        Deactivated,
        Ungenerated,
        AwaitingBuilding,
        Unmeshed,
        AwaitingMeshing,
        Finished
    }
}
