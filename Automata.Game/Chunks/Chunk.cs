#region

using System;
using System.Collections.Generic;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Entities;

#endregion


namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; }
        public INodeCollection<ushort>? Blocks { get; set; }
        public IEnumerable<IEntity>? NeighborEntities { get; set; }
    }

    public enum GenerationState
    {
        Deactivated,
        Ungenerated,
        AwaitingBuilding,
        AwaitingMeshing,
        Finished
    }
}
