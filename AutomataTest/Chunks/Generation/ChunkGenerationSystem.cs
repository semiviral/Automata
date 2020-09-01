#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Automata.Collections;
using Automata.Entity;
using Automata.Extensions;
using Automata.Numerics;
using Automata.Rendering.Meshes;
using Automata.Rendering.OpenGL;
using Automata.System;
using ConcurrentAsyncScheduler;
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

            HandledComponentTypes = new ComponentTypes(typeof(Translation), typeof(ChunkState), typeof(BlocksCollection));
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
                    {
                        void BeginChunkBuilding(ChunkID id0, ChunkState state0, Translation translation)
                        {
                            ChunkBuildingJob chunkBuildingJob = _ChunkBuilders.Retrieve() ?? new ChunkBuildingJob();
                            chunkBuildingJob.SetData(Vector3i.FromVector3(translation.Value), GenerationConstants.Seed, 0.01f, 1f);

                            // local method for building finished
                            void OnChunkBuildingFinished(object? sender, AsyncJob asyncJob)
                            {
                                Log.Verbose($"Chunk building finished for chunk '{id0.Value}' ({asyncJob.ExecutionTime.Milliseconds}ms).");

                                asyncJob.WorkFinished -= OnChunkBuildingFinished;

                                if (state0.Value == GenerationState.Deactivated)
                                {
                                    return;
                                }

                                ChunkBuildingJob finishedChunkBuildingJob = (ChunkBuildingJob)asyncJob;
                                _FinishedBuildingJobs.AddOrUpdate(id0.Value, finishedChunkBuildingJob, (guid, job) => finishedChunkBuildingJob);
                            }


                            chunkBuildingJob.WorkFinished += OnChunkBuildingFinished;

                            AsyncJobScheduler.QueueAsyncJob(chunkBuildingJob);
                        }

                        BeginChunkBuilding(id, state, entity.GetComponent<Translation>());

                        state.Value = state.Value.Next();
                    }
                        break;
                    case GenerationState.AwaitingBuilding:
                    {
                        if (_FinishedBuildingJobs.TryRemove(id.Value, out ChunkBuildingJob? chunkBuildingJob))
                        {
                            entity.GetComponent<BlocksCollection>().Value = chunkBuildingJob.GetGeneratedBlockData();
                            chunkBuildingJob.ClearData();
                            _ChunkBuilders.TryAdd(chunkBuildingJob);

                            state.Value = state.Value.Next();
                        }
                    }
                        break;
                    case GenerationState.Unmeshed:
                    {
                        void BeginChunkMeshing(ChunkID id0, ChunkState state0, BlocksCollection blocksCollection)
                        {
                            Debug.Assert(state0.Value == GenerationState.Unmeshed);

                            ChunkMeshingJob chunkMeshingJob = _ChunkMeshers.Retrieve() ?? new ChunkMeshingJob();
                            chunkMeshingJob.SetData(blocksCollection.Value, new INodeCollection<ushort>[0]);

                            void OnChunkMeshingFinished(object? sender, AsyncJob asyncJob)
                            {
                                Log.Verbose($"Chunk meshing finished for chunk '{id0.Value}' ({asyncJob.ExecutionTime.Milliseconds}ms).");

                                asyncJob.WorkFinished -= OnChunkMeshingFinished;

                                if (state0.Value == GenerationState.Deactivated)
                                {
                                    return;
                                }

                                ChunkMeshingJob finishedChunkMeshingJob = (ChunkMeshingJob)asyncJob;
                                _FinishedMeshingJobs.AddOrUpdate(id0.Value, finishedChunkMeshingJob, (guid, job) => finishedChunkMeshingJob);
                            }

                            chunkMeshingJob.WorkFinished += OnChunkMeshingFinished;

                            AsyncJobScheduler.QueueAsyncJob(chunkMeshingJob);
                        }


                        BeginChunkMeshing(id, state, entity.GetComponent<BlocksCollection>());

                        state.Value = state.Value.Next();
                    }
                        break;
                    case GenerationState.AwaitingMeshing:
                    {
                        if (_FinishedMeshingJobs.TryRemove(id.Value, out ChunkMeshingJob? chunkMeshingJob))
                        {
                            if (!entity.TryGetComponent(out RenderMesh renderMesh))
                            {
                                renderMesh = new RenderMesh();

                                entityManager.RegisterComponent(entity, renderMesh);
                            }

                            Mesh<float> mesh = new Mesh<float>();
                            mesh.VertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0);
                            mesh.VertexesBuffer.SetBufferData(StaticCube.Vertexes);
                            mesh.IndexesBuffer.SetBufferData(StaticCube.Indexes);

                            renderMesh.Mesh = mesh;

                            chunkMeshingJob.ClearData();
                            _ChunkMeshers.TryAdd(chunkMeshingJob);

                            state.Value = state.Value.Next();
                        }
                    }
                        break;
                    case GenerationState.Meshed:
                    case GenerationState.Deactivated:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
