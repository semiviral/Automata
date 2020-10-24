#region

using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Systems;

#endregion


namespace Automata.Game.Chunks
{
    public class ChunkComposition : IEntityComposition
    {
        public ChunkComposition() => ComposedTypes = new ComponentTypes((DistinctionStrategy)0, typeof(Translation), typeof(Chunk), typeof(Bounds));
        public ComponentTypes ComposedTypes { get; }
    }
}
