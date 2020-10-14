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
        private static readonly ObjectPool<ChunkMeshingJob> _ChunkMeshingJob = new ObjectPool<ChunkMeshingJob>(() => new ChunkMeshingJob());


        private readonly OrderedList<BuildStep> _BuildSteps;
        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _GeneratedBlockCollections;
        private readonly ConcurrentDictionary<Guid, ChunkMeshingJob> _FinishedMeshingJobs;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedList<BuildStep>();
            _BuildSteps.AddLast(new TerrainBuildStep());
            _FinishedMeshingJobs = new ConcurrentDictionary<Guid, ChunkMeshingJob>();
            _FinishedMeshingJobs = new ConcurrentDictionary<Guid, ChunkMeshingJob>();

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
                    case GenerationState.Unbuilt:
                        BuildStep.Parameters parameters = new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY,
                            GenerationConstants.PERSISTENCE, new Vector3i(0, GenerationConstants.WORLD_HEIGHT / 3, 0));
                        AsyncJobScheduler.QueueAsyncInvocation(() => BuildChunk(id.Value, parameters, _BuildSteps));

                        //state.Value = state.Value.Next();
                        break;
                    case GenerationState.AwaitingBuilding:
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

        private Task BuildChunk(Guid chunkId, BuildStep.Parameters parameters, IEnumerable<BuildStep> buildSteps)
        {
            static INodeCollection<ushort> GenerateNodeCollectionImpl(ref Span<ushort> blocks)
            {
                Octree<ushort> blocksCompressed = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, BlockRegistry.AirID, false);

                int index = 0;
                for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
                {
                    blocksCompressed.SetPoint(x, y, z, blocks[index]);
                }

                return blocksCompressed;
            }

            if (parameters is null || buildSteps is null)
            {
                throw new InvalidOperationException("Job data has not been provided.");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            foreach (BuildStep generationStep in buildSteps)
            {
                generationStep.Generate(parameters, ref blocks);
            }

            INodeCollection<ushort> nodeCollection = GenerateNodeCollectionImpl(ref blocks);
            _GeneratedBlockCollections.AddOrUpdate(chunkId, nodeCollection, (guid, collection) => nodeCollection);

            Diagnostics.Instance["ChunkBuilding"].Enqueue(stopwatch.Elapsed);
            double time = Diagnostics.Instance.GetAverageTime("ChunkBuilding").TotalMilliseconds;
            Log.Information($"Built in {time:0.00}ms");

            return Task.CompletedTask;
        }
    }
}
