#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        private readonly ConcurrentDictionary<Guid, PendingMesh<int>> _FinishedMeshes;

        public ChunkGenerationSystem()
        {
            _FinishedBlocksCollections = new ConcurrentDictionary<Guid, INodeCollection<ushort>>();
            _FinishedMeshes = new ConcurrentDictionary<Guid, PendingMesh<int>>();

            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(ChunkState),
                typeof(BlocksCollection),
            };
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<ChunkID, Translation, ChunkState, BlocksCollection>())
            {
                ChunkState state = entity.GetComponent<ChunkState>();

                switch (state.Value)
                {
                    case GenerationState.Deactivated:
                    {
                        ChunkID id = entity.GetComponent<ChunkID>();

                        if (_FinishedBlocksCollections.ContainsKey(id.Value))
                        {
                            _FinishedBlocksCollections.TryRemove(id.Value, out INodeCollection<ushort> _);
                        }

                        break;
                    }
                    case GenerationState.Unbuilt:
                    {
                        BeginChunkBuilding(entity.GetComponent<ChunkID>(), state, entity.GetComponent<Translation>());

                        state.Value = state.Value.Next();

                        break;
                    }
                    case GenerationState.AwaitingBuilding:
                    {
                        if (_FinishedBlocksCollections.TryGetValue(entity.GetComponent<ChunkID>().Value, out INodeCollection<ushort>? blocks))
                        {
                            BlocksCollection blocksCollection = entity.GetComponent<BlocksCollection>();
                            blocksCollection.Value = blocks;
                            state.Value = state.Value.Next();
                        }

                        break;
                    }
                    case GenerationState.Unmeshed:
                    {
                        BeginChunkMeshing(entity.GetComponent<ChunkID>(), state, entity.GetComponent<BlocksCollection>());

                        state.Value = state.Value.Next();
                        break;
                    }
                    case GenerationState.AwaitingMeshing:
                    {
                        ChunkID id = entity.GetComponent<ChunkID>();

                        if (_FinishedMeshes.TryGetValue(id.Value, out PendingMesh<int>? mesh))
                        {
                            _FinishedMeshes.TryRemove(id.Value, out mesh);

                            Debug.Assert(mesh != null);

                            entityManager.RegisterComponent(entity, mesh);
                        }

                        break;
                    }
                    case GenerationState.Meshed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void BeginChunkBuilding(ChunkID id, ChunkState state, Translation translation)
        {
            ChunkBuildingJob chunkBuildingJob = _ChunkBuilders.Retrieve() ?? new ChunkBuildingJob();
            chunkBuildingJob.SetData(Vector3i.FromVector3(translation.Value), GenerationConstants.Seed, 0.01f, 1f);

            // local method for building finished
            void OnChunkBuildingFinished(object? sender, AsyncJob asyncJob)
            {
                asyncJob.WorkFinished -= OnChunkBuildingFinished;

                if (state.Value == GenerationState.Deactivated)
                {
                    return;
                }

                ChunkBuildingJob finishedChunkBuildingJob = (ChunkBuildingJob)asyncJob;
                _FinishedBlocksCollections.TryAdd(id.Value, finishedChunkBuildingJob.GetGeneratedBlockData());
                finishedChunkBuildingJob.ClearData();

                _ChunkBuilders.TryAdd(finishedChunkBuildingJob);
            }

            chunkBuildingJob.WorkFinished += OnChunkBuildingFinished;

            AsyncJobScheduler.QueueAsyncJob(chunkBuildingJob);
        }

        private void BeginChunkMeshing(ChunkID id, ChunkState state, BlocksCollection blocksCollection)
        {
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

                ChunkMeshingJob finishedChunkMeshingJob = (ChunkMeshingJob)asyncJob;
                _FinishedMeshes.TryAdd(id.Value, finishedChunkMeshingJob.GetData());
                finishedChunkMeshingJob.ClearData();

                _ChunkMeshers.TryAdd(finishedChunkMeshingJob);
            }

            chunkMeshingJob.WorkFinished += OnChunkMeshingFinished;

            AsyncJobScheduler.QueueAsyncJob(chunkMeshingJob);
        }
    }
}
