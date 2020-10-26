#region

using System;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;

#endregion


namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        private static readonly INodeCollection<ushort>?[] _NoNeighborBlocks = new INodeCollection<ushort>?[6];

        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; }
        public INodeCollection<ushort>? Blocks { get; set; }
        public Chunk?[]? Neighbors { get; set; }

        public INodeCollection<ushort>?[] GetNeighborBlocks() => Neighbors?.Select(neighbor => neighbor?.Blocks).ToArray() ?? _NoNeighborBlocks;

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
