#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata;
using Automata.Collections;
using Automata.Components;
using Automata.Entities;
using Automata.Extensions;
using Automata.Numerics;
using Automata.Rendering.Meshes;
using Automata.Systems;
using AutomataTest.Blocks;
using ConcurrentAsyncScheduler;
using Serilog;
using Silk.NET.OpenGL;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private class ChunkBuildingJob : AsyncJob
        {
            public BuildStep.Parameters? Parameters { get; set; }
            public OrderedList<BuildStep>? GenerationSteps { get; set; }
            public INodeCollection<ushort>? Blocks { get; set; }

            protected override Task Process()
            {
                if (Parameters is null || GenerationSteps is null)
                {
                    throw new InvalidOperationException("Job data has not been provided.");
                }

                Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

                foreach (BuildStep generationStep in GenerationSteps)
                {
                    generationStep.Generate(Parameters, ref blocks);
                }

                Octree<ushort> blocksCompressed = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, BlockRegistry.AirID, false);

                int index = 0;
                for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++,  index++)
                {
                    blocksCompressed.SetPoint(x, y, z, blocks[index]);
                }

                Blocks = blocksCompressed;
                Parameters = default;
                GenerationSteps = default;

                return Task.CompletedTask;
            }
        }

        private static readonly ObjectPool<ChunkBuildingJob> _ChunkBuildingJobs = new ObjectPool<ChunkBuildingJob>(() => new ChunkBuildingJob());
        private static readonly ObjectPool<ChunkMeshingJob> _ChunkMeshingJob = new ObjectPool<ChunkMeshingJob>(() => new ChunkMeshingJob());

        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _GeneratedBlockCollections;
        private readonly ConcurrentDictionary<Guid, ChunkMeshingJob> _FinishedMeshingJobs;

        private readonly OrderedList<BuildStep> _BuildSteps;

        public ChunkGenerationSystem()
        {
            _GeneratedBlockCollections = new ConcurrentDictionary<Guid, INodeCollection<ushort>>();
            _FinishedMeshingJobs = new ConcurrentDictionary<Guid, ChunkMeshingJob>();
            _BuildSteps = new OrderedList<BuildStep>();
            _BuildSteps.AddLast(new TerrainBuildStep());
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("ChunkBuilding");

            HandledComponents = new ComponentTypes(typeof(Translation), typeof(ChunkState), typeof(BlocksCollection));
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<ChunkID, Translation, ChunkState, BlocksCollection>())
            {
                ChunkID id = entity.GetComponent<ChunkID>();
                ChunkState state = entity.GetComponent<ChunkState>();

                switch (state.Value)
                {
                    case GenerationState.Ungenerated:
                        ChunkBuildingJob chunkBuildingJob = _ChunkBuildingJobs.Rent();
                        chunkBuildingJob.Parameters = new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY,
                            GenerationConstants.PERSISTENCE, Vector3i.FromVector3(entity.GetComponent<Translation>().Value));
                        chunkBuildingJob.GenerationSteps = _BuildSteps;

                        void OnChunkGenerationFinished(object? sender, AsyncJob asyncJob)
                        {
                            asyncJob.Finished -= OnChunkGenerationFinished;

                            if ((state.Value == GenerationState.Deactivated)
                                || !(asyncJob is ChunkBuildingJob buildingJob)
                                || buildingJob.Blocks is null)
                            {
                                return;
                            }

                            _GeneratedBlockCollections.AddOrUpdate(id.Value, buildingJob.Blocks as INodeCollection<ushort>,
                                (guid, collection) => buildingJob.Blocks);
                            buildingJob.Blocks = default;

                            Diagnostics.Instance["ChunkBuilding"].Enqueue(buildingJob.ProcessTime);
                            Log.Information($"{Diagnostics.Instance.GetAverageTime("ChunkBuilding").TotalMilliseconds:0.00}ms");

                            _ChunkBuildingJobs.Return(buildingJob);
                        }

                        chunkBuildingJob.Finished += OnChunkGenerationFinished;

                        AsyncJobScheduler.QueueAsyncJob(chunkBuildingJob);

                        //state.Value = state.Value.Next();
                        break;
                    case GenerationState.AwaitingGeneration:
                    {
                        if (_GeneratedBlockCollections.TryRemove(id.Value, out INodeCollection<ushort>? blocks))
                        {
                            entity.GetComponent<BlocksCollection>().Value = blocks;
                            state.Value = state.Value.Next();
                        }
                    }
                        break;
                    case GenerationState.Unmeshed:
                    {
                        void BeginChunkMeshing(ChunkID id0, ChunkState state0, BlocksCollection blocksCollection)
                        {
                            Debug.Assert(state0.Value == GenerationState.Unmeshed);

                            ChunkMeshingJob chunkMeshingJob = _ChunkMeshingJob.Rent();
                            chunkMeshingJob.SetData(blocksCollection.Value, new INodeCollection<ushort>[0]);

                            void OnChunkMeshingFinished(object? sender, AsyncJob asyncJob)
                            {
                                Log.Verbose($"({nameof(ChunkGenerationSystem)}) Meshed: '{id0.Value}' ({asyncJob.ExecutionTime.Milliseconds}ms).");

                                asyncJob.Finished -= OnChunkMeshingFinished;

                                if (state0.Value == GenerationState.Deactivated)
                                {
                                    return;
                                }

                                ChunkMeshingJob finishedChunkMeshingJob = (ChunkMeshingJob)asyncJob;
                                _FinishedMeshingJobs.AddOrUpdate(id0.Value, finishedChunkMeshingJob, (guid, job) => finishedChunkMeshingJob);
                            }

                            chunkMeshingJob.Finished += OnChunkMeshingFinished;

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

                            PendingMesh<int> pendingMesh = chunkMeshingJob.GetData();
                            Mesh<int> packedMesh = new Mesh<int>();
                            int[] vertexes = pendingMesh.Vertexes.ToArray();
                            uint[] indexes = pendingMesh.Indexes.ToArray();

                            Log.Verbose($"({nameof(ChunkGenerationSystem)} Chunk meshed: {vertexes.Length} vertexes, {indexes.Length} indexes");

                            packedMesh.VertexArrayObject.VertexAttributeIPointer(0, 1, VertexAttribPointerType.Int, 0);
                            packedMesh.VertexesBuffer.SetBufferData(vertexes);
                            packedMesh.IndexesBuffer.SetBufferData(indexes);
                            renderMesh.Mesh = packedMesh;

                            chunkMeshingJob.ClearData();
                            _ChunkMeshingJob.Return(chunkMeshingJob);

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
