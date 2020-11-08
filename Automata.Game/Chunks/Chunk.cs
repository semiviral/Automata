using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;

namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; } = GenerationState.Ungenerated;
        public Palette<ushort>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];

        public IEnumerable<Palette<ushort>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public GenerationState MinimalNeighborState() => Neighbors?.Min(neighbor => neighbor?.State ?? GenerationState.Finished)
                                                         ?? GenerationState.Ungenerated;
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
