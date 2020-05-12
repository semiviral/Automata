using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;

namespace AutomataTest.Chunks
{
    public class ChunkBuildingSystem : ComponentSystem
    {
        public ChunkBuildingSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(ChunkGenerationState),
                typeof(ChunkData),
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Translation translation, ChunkGenerationState state, ChunkData blockData) in entityManager
                .GetComponents<Translation, ChunkGenerationState, ChunkData>())
            {

            }
        }
    }
}
