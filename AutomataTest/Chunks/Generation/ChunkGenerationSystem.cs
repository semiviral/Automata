#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Automata;
using Automata.Collections;
using Automata.Components;
using Automata.Entities;
using Automata.Extensions;
using Automata.Numerics;
using Automata.Rendering.Meshes;
using Automata.Systems;
using AutomataTest.Blocks;
using ConcurrentPools;
using Serilog;
using Silk.NET.OpenGL;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private readonly OrderedList<BuildStep> _BuildSteps;
        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _PendingBlockCollections;
        private readonly ConcurrentDictionary<Guid, PendingMesh<int>> _PendingMeshes;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedList<BuildStep>();
            _BuildSteps.AddLast(new TerrainBuildStep());
            _PendingBlockCollections = new ConcurrentDictionary<Guid, INodeCollection<ushort>>();
            _PendingMeshes = new ConcurrentDictionary<Guid, PendingMesh<int>>();

            Diagnostics.Instance.RegisterDiagnosticTimeEntry("ChunkBuilding");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("ChunkMeshing");

            BoundedThreadPool.DefaultThreadPoolSize();

            HandledComponents = new ComponentTypes(typeof(Translation), typeof(ChunkState), typeof(BlocksCollection));
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<ChunkID, Translation, ChunkState, BlocksCollection>())
            {
                ChunkID chunkID = entity.GetComponent<ChunkID>();
                ChunkState chunkState = entity.GetComponent<ChunkState>();
                BlocksCollection blocksCollection = entity.GetComponent<BlocksCollection>();

                switch (chunkState.Value)
                {
                    case GenerationState.Ungenerated:
                        BuildStep.Parameters parameters = new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY,
                            GenerationConstants.PERSISTENCE, new Vector3i(0, GenerationConstants.WORLD_HEIGHT / 3, 0));
                        BoundedThreadPool.QueueWork(() => GenerateChunk(chunkID.Value, parameters));

                        chunkState.Value = chunkState.Value.Next();
                        break;
                    case GenerationState.AwaitingBuilding:
                        if (_PendingBlockCollections.TryRemove(chunkID.Value, out INodeCollection<ushort>? nodeCollection))
                        {
                            blocksCollection.Value = nodeCollection;
                            chunkState.Value = chunkState.Value.Next();
                        }

                        break;
                    case GenerationState.AwaitingMeshing:
                        if (_PendingMeshes.TryRemove(chunkID.Value, out PendingMesh<int>? pendingMesh))
                        {
                            if (!entity.TryGetComponent(out RenderMesh renderMesh))
                            {
                                renderMesh = new RenderMesh();

                                entityManager.RegisterComponent(entity, renderMesh);
                            }

                            Mesh<int> mesh = new Mesh<int>();
                            mesh.VertexArrayObject.VertexAttributeIPointer(0, 1, VertexAttribPointerType.Int, 0);
                            mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes);
                            mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes);
                            renderMesh.Mesh = mesh;

                            chunkState.Value = chunkState.Value.Next();
                        }

                        break;
                    case GenerationState.Finished:
                    case GenerationState.Deactivated:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void GenerateChunk(Guid chunkID, BuildStep.Parameters parameters)
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

            if (parameters is null || _BuildSteps is null)
            {
                throw new InvalidOperationException("Job data has not been provided.");
            }


            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (BuildStep generationStep in _BuildSteps)
            {
                generationStep.Generate(parameters, blocks);
            }

            INodeCollection<ushort> nodeCollection = GenerateNodeCollectionImpl(ref blocks);
            _PendingBlockCollections.AddOrUpdate(chunkID, nodeCollection, (guid, collection) => nodeCollection);

            stopwatch.Stop();

            Diagnostics.Instance["ChunkBuilding"].Enqueue(stopwatch.Elapsed);
            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Built: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            PendingMesh<int> pendingMesh = ChunkMesher.GenerateMesh(blocks, new INodeCollection<ushort>[6]);
            _PendingMeshes.AddOrUpdate(chunkID, pendingMesh, (guid, mesh) => pendingMesh);

            stopwatch.Stop();

            Diagnostics.Instance["ChunkMeshing"].Enqueue(stopwatch.Elapsed);
            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));
        }
    }
}
