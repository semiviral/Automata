#region

using Automata.Components;
using Automata.Entities;
using Automata.Systems;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkComposition : IEntityComposition
    {
        public ComponentTypes ComposedTypes { get; }

        public ChunkComposition() => ComposedTypes = new ComponentTypes(typeof(Translation), typeof(ChunkID), typeof(ChunkState),
            typeof(BlocksCollection));
    }
}
