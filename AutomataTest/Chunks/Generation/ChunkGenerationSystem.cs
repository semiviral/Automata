#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Automata.Collections;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Extensions;
using Automata.Jobs;
using Automata.Numerics;
using Automata.Rendering;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private static readonly ObjectPool<ChunkBuildingJob> _ChunkBuilders = new ObjectPool<ChunkBuildingJob>();
        private static readonly ObjectPool<ChunkMeshingJob> _ChunkMeshers = new ObjectPool<ChunkMeshingJob>();

        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _FinishedBlocksCollections;
        private readonly ConcurrentDictionary<Guid, PendingMesh<float>> _FinishedMeshes;

        public ChunkGenerationSystem()
        {
            _FinishedBlocksCollections = new ConcurrentDictionary<Guid, INodeCollection<ushort>>();
            _FinishedMeshes = new ConcurrentDictionary<Guid, PendingMesh<float>>();

            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(ChunkState),
                typeof(BlocksCollection),
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<ChunkID, Translation, ChunkState, BlocksCollection>())
            {
                ChunkState state = entity.GetComponent<ChunkState>();
                ChunkID id = entity.GetComponent<ChunkID>();

                switch (state.Value)
                {
                    case GenerationState.Deactivated:
                        if (_FinishedBlocksCollections.ContainsKey(id.Value))
                        {
                            _FinishedBlocksCollections.TryRemove(id.Value, out INodeCollection<ushort> _);
                        }

                        break;
                    case GenerationState.Unbuilt:
                        Translation translation = entity.GetComponent<Translation>();

                        ChunkBuildingJob buildingJob = _ChunkBuilders.Retrieve() ?? new ChunkBuildingJob();
                        buildingJob.SetData(Vector3i.FromVector3(translation.Value), GenerationConstants.Seed, 0.01f, 1f);

                        // local method for building finished
                        void OnChunkBuildingFinished(object? sender, AsyncJob asyncJob)
                        {
                            asyncJob.WorkFinished -= OnChunkBuildingFinished;

                            if (state.Value == GenerationState.Deactivated)
                            {
                                return;
                            }

                            _FinishedBlocksCollections.TryAdd(id.Value, ((ChunkBuildingJob)asyncJob).GetGeneratedBlockData());
                        }

                        buildingJob.WorkFinished += OnChunkBuildingFinished;

                        AsyncJobScheduler.QueueAsyncJob(buildingJob);

                        // set state to 'awaiting building'
                        state.Value = state.Value.Next();
                        break;
                    case GenerationState.AwaitingBuilding:
                        if (_FinishedBlocksCollections.TryGetValue(id.Value, out INodeCollection<ushort>? blocks))
                        {
                            BlocksCollection blocksCollection = entity.GetComponent<BlocksCollection>();
                            blocksCollection.Value = blocks;
                            state.Value = state.Value.Next();
                        }

                        break;
                    case GenerationState.Unmeshed:
                        Debug.Assert(state.Value == GenerationState.Unmeshed);

                        ChunkMeshingJob chunkMeshingJob = _ChunkMeshers.Retrieve() ?? new ChunkMeshingJob();
                        chunkMeshingJob.SetData(blocksCollection.Value, new INodeCollection<ushort>[0]);

                        void OnChunkMeshingFinished(object? sender, AsyncJob asyncJob)
                        {
                            asyncJob.WorkFinished -= OnChunkMeshingFinished;

                            if (state.Value == GenerationState.Deactivated)
                            {
                                return;
                            }

                            // set state to 'meshed'
                            state.Value = state.Value.Next();
                        }

                        break;
                    case GenerationState.AwaitingMeshing:
                        if (_FinishedMeshes.TryGetValue(id.Value, out PendingMesh<float>? mesh))
                        {
                            _FinishedMeshes.TryRemove(id.Value, out mesh);

                            Debug.Assert(mesh != null);

                            entityManager.RegisterComponent(entity, mesh);
                        }
                        break;
                    case GenerationState.Meshed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
