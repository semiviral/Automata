#region

using System;
using System.Linq;
using Automata.Engine.Components;
using Generic_Octree;

#endregion


namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; }
        public INodeCollection<ushort>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; set; } = new Chunk?[6];
        public INodeCollection<ushort>?[] NeighborBlocks { get; set; } = new INodeCollection<ushort>?[6];

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
