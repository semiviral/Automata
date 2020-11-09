using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; } = GenerationState.Ungenerated;
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];

        public IEnumerable<Palette<Block>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public bool IsStateLockstep() => Neighbors.All(chunk => chunk is null || chunk.State >= State);
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
