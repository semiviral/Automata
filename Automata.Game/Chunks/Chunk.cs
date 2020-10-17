#region

using System;
using Automata.Engine.Collections;
using Automata.Engine.Components;

#endregion

namespace Automata.Game.Chunks
{
    public class Chunk : IComponent
    {
        public Guid ID { get; } = Guid.NewGuid();
        public GenerationState State { get; set; }
        public INodeCollection<ushort>? Blocks { get; set; }
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
