#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Collections;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Extensions;
using Automata.Jobs;
using Automata.Numerics;
using Automata.Rendering;
using Serilog;
using Silk.NET.OpenGL;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private static readonly ObjectPool<ChunkBuildingJob> _ChunkBuilders = new ObjectPool<ChunkBuildingJob>();
        private static readonly ObjectPool<ChunkMeshingJob> _ChunkMeshers = new ObjectPool<ChunkMeshingJob>();

        private readonly ConcurrentDictionary<Guid, ChunkBuildingJob> _FinishedBuildingJobs;
        private readonly ConcurrentDictionary<Guid, ChunkMeshingJob> _FinishedMeshingJobs;
        private readonly HashSet<Guid> _ProcessedDeactivatedChunks;

        public ChunkGenerationSystem()
        {
            _FinishedBuildingJobs = new ConcurrentDictionary<Guid, ChunkBuildingJob>();
            _FinishedMeshingJobs = new ConcurrentDictionary<Guid, ChunkMeshingJob>();
            _ProcessedDeactivatedChunks = new HashSet<Guid>();

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
                ChunkID id = entity.GetComponent<ChunkID>();
                ChunkState state = entity.GetComponent<ChunkState>();

                if (state.Value == GenerationState.Deactivated)
                {
                    if (_ProcessedDeactivatedChunks.Contains(id.Value))
                    {
                        _ProcessedDeactivatedChunks.Add(id.Value);
                    }

                    return;
                }
                else if ((state.Value != GenerationState.Deactivated) && _ProcessedDeactivatedChunks.Contains(id.Value))
                {
                    _ProcessedDeactivatedChunks.Remove(id.Value);
                }

                switch (state.Value)
                {
                    case GenerationState.Unbuilt:
                        BeginChunkBuilding(id, state, entity.GetComponent<Translation>());

                        state.Value = state.Value.Next();

                        break;
                    case GenerationState.AwaitingBuilding:
                        if (_FinishedBuildingJobs.TryRemove(id.Value, out ChunkBuildingJob? chunkBuildingJob))
                        {
                            entity.GetComponent<BlocksCollection>().Value = chunkBuildingJob.GetGeneratedBlockData();
                            chunkBuildingJob.ClearData();
                            _ChunkBuilders.TryAdd(chunkBuildingJob);

                            state.Value = state.Value.Next();
                        }

                        break;
                    case GenerationState.Unmeshed:
                        BeginChunkMeshing(id, state, entity.GetComponent<BlocksCollection>());

                        state.Value = state.Value.Next();
                        break;
                    case GenerationState.AwaitingMeshing:
                        if (_FinishedMeshingJobs.TryRemove(id.Value, out ChunkMeshingJob? chunkMeshingJob))
                        {
                            if (!entity.TryGetComponent(out PackedMesh packedMesh))
                            {
                                packedMesh = new PackedMesh();

                                entityManager.RegisterComponent(entity, packedMesh);
                            }

                            PendingMesh<int> pendingMesh = chunkMeshingJob.GetData();
                            packedMesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes.ToArray());
                            packedMesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes.ToArray());
                            packedMesh.VertexArrayObject.VertexAttributePointer(0, 1, VertexAttribPointerType.Int, 1, 0);

                            chunkMeshingJob.ClearData();
                            _ChunkMeshers.TryAdd(chunkMeshingJob);

                            state.Value = state.Value.Next();
                        }

                        break;
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
            chunkBuildingJob.SetData(Vector3i.FromVector3d(translation.Value), GenerationConstants.Seed, 0.01f, 1f);

            // local method for building finished
            void OnChunkBuildingFinished(object? sender, AsyncJob asyncJob)
            {
                Log.Verbose($"Chunk building finished for chunk '{id.Value}' ({asyncJob.ExecutionTime.Milliseconds}ms).");

                asyncJob.WorkFinished -= OnChunkBuildingFinished;

                if (state.Value == GenerationState.Deactivated)
                {
                    return;
                }

                ChunkBuildingJob finishedChunkBuildingJob = (ChunkBuildingJob)asyncJob;
                _FinishedBuildingJobs.AddOrUpdate(id.Value, finishedChunkBuildingJob, (guid, job) => finishedChunkBuildingJob);
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
                Log.Verbose($"Chunk meshing finished for chunk '{id.Value}' ({asyncJob.ExecutionTime.Milliseconds}ms).");

                asyncJob.WorkFinished -= OnChunkMeshingFinished;

                if (state.Value == GenerationState.Deactivated)
                {
                    return;
                }

                ChunkMeshingJob finishedChunkMeshingJob = (ChunkMeshingJob)asyncJob;
                _FinishedMeshingJobs.AddOrUpdate(id.Value, finishedChunkMeshingJob, (guid, job) => finishedChunkMeshingJob);
            }

            chunkMeshingJob.WorkFinished += OnChunkMeshingFinished;

            AsyncJobScheduler.QueueAsyncJob(chunkMeshingJob);
        }
    }
}
