using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;

namespace AutomataTest
{
    public class ChunkBuildingSystem : ComponentSystem
    {
        public ChunkBuildingSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(ChunkGenerationState),
                typeof(BlockData),
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Translation translation, ChunkGenerationState state, BlockData blockData) in entityManager
                .GetComponents<Translation, ChunkGenerationState, BlockData>())
            {

            }
        }
    }
}
