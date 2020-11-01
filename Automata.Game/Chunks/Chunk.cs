#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Components;
using Generic_Octree;

#endregion


namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; } = GenerationState.Ungenerated;
        public INodeCollection<ushort>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];

        public IEnumerable<INodeCollection<ushort>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

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
