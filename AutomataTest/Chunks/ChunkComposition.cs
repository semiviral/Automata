#region

using Automata.Components;
using Automata.Entities;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkComposition : IEntityComposition
    {
        public IEntity ComposeEntity()
        {
            Entity chunk = new Entity();
            chunk.AddComponent(new Translation());
            chunk.AddComponent(new ChunkID());
            chunk.AddComponent(new ChunkState
            {
                Value = GenerationState.Ungenerated
            });
            chunk.AddComponent(new BlocksCollection());

            return chunk;
        }
    }
}
