#region

using System.Diagnostics;
using Automata.Collections;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Extensions;
using Automata.Jobs;
using Automata.Numerics;
using AutomataTest.Chunks.Generation;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkBuildingSystem : ComponentSystem
    {
        private static readonly ObjectPool<ChunkBuildingJob> _TerrainBuilders = new ObjectPool<ChunkBuildingJob>();

        public ChunkBuildingSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(GenerationState),
                typeof(BlocksCollection),
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Translation translation, GenerationState generationState, BlocksCollection blockCollection) in
                entityManager.GetComponents<Translation, GenerationState, BlocksCollection>())
            {
                if (generationState.State > ChunkState.Unbuilt)
                {
                    continue;
                }

                ChunkBuildingJob buildingJob = _TerrainBuilders.Retrieve() ?? new ChunkBuildingJob();
                buildingJob.SetData(Vector3i.FromVector3(translation.Value), GenerationConstants.Seed, 0.01f, 1f);

                void OnTerrainBuildingFinished(object? sender, AsyncJob asyncJob)
                {
                    asyncJob.WorkFinished -= OnTerrainBuildingFinished;

                    Debug.Assert(generationState.State == ChunkState.AwaitingBuilding);

                    generationState.State = generationState.State.Next();

                    blockCollection.Blocks = buildingJob.GetGeneratedBlockData();
                }

                buildingJob.WorkFinished += OnTerrainBuildingFinished;

                generationState.State = generationState.State.Next();

                AsyncJobScheduler.QueueAsyncJob(buildingJob);
            }
        }
    }
}
