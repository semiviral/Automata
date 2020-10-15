#region

using Automata.Components;
using Automata.Entities;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkComposition : IEntityComposition
    {
        public IEntity ComposeEntity(EntityManager entityManager)
        {
            Entity chunk = new Entity();
            entityManager.RegisterEntity(chunk);
            entityManager.RegisterComponent<Translation>(chunk);
            entityManager.RegisterComponent(chunk, new ChunkState
            {
                Value = GenerationState.Ungenerated
            });
            entityManager.RegisterComponent<ChunkID>(chunk);
            entityManager.RegisterComponent<BlocksCollection>(chunk);

            return chunk;
        }
    }
}
