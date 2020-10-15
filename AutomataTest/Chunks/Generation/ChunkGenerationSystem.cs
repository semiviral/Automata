#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Automata;
using Automata.Collections;
using Automata.Components;
using Automata.Diagnostics;
using Automata.Entities;
using Automata.Extensions;
using Automata.Input;
using Automata.Numerics;
using Automata.Rendering.Meshes;
using Automata.Systems;
using AutomataTest.Blocks;
using ConcurrentPools;
using Serilog;
using Silk.NET.Input.Common;
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

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();

            HandledComponents = new ComponentTypes(typeof(Translation), typeof(ChunkState), typeof(BlocksCollection));
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<ChunkID, Translation, ChunkState, BlocksCollection>())
            {
                ChunkID chunkID = entity.GetComponent<ChunkID>();
                ChunkState chunkState = entity.GetComponent<ChunkState>();

                switch (chunkState.Value)
                {
                    case GenerationState.Ungenerated:
                        Translation translation = entity.GetComponent<Translation>();
                        BuildStep.Parameters parameters = new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY,
                            GenerationConstants.PERSISTENCE, Vector3i.FromVector3(translation.Value));
                        BoundedThreadPool.QueueWork(() => GenerateChunk(chunkID.Value, parameters));

                        chunkState.Value = chunkState.Value.Next();
                        break;
                    case GenerationState.AwaitingBuilding:
                        if (_PendingBlockCollections.TryRemove(chunkID.Value, out INodeCollection<ushort>? nodeCollection))
                        {
                            entity.GetComponent<BlocksCollection>().Value = nodeCollection;
                            chunkState.Value = chunkState.Value.Next();
                        }

                        break;
                    case GenerationState.AwaitingMeshing:
                        if (_PendingMeshes.TryRemove(chunkID.Value, out PendingMesh<int>? pendingMesh))
                        {
                            Stopwatch stopwatch = DiagnosticsProvider.Stopwatches.Rent();
                            stopwatch.Restart();

                            if (!entity.TryGetComponent(out RenderMesh renderMesh))
                            {
                                renderMesh = new RenderMesh();

                                entityManager.RegisterComponent(entity, renderMesh);
                            }

                            Mesh<int> mesh = new Mesh<int>();
                            mesh.VertexArrayObject.VertexAttributeIPointer(0u, 1, VertexAttribPointerType.Int, 0);
                            mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes);
                            mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes);
                            renderMesh.Mesh = mesh;

                            stopwatch.Stop();

                            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
                            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                                $"Applied mesh: '{chunkID.Value}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

                            DiagnosticsProvider.Stopwatches.Return(stopwatch);

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

            DiagnosticsInputCheck();
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

            Stopwatch stopwatch = DiagnosticsProvider.Stopwatches.Rent();
            stopwatch.Restart();

            foreach (BuildStep generationStep in _BuildSteps)
            {
                generationStep.Generate(parameters, blocks);
            }

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new BuildingTime(stopwatch.Elapsed));
            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Built: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            INodeCollection<ushort> nodeCollection = GenerateNodeCollectionImpl(ref blocks);
            _PendingBlockCollections.AddOrUpdate(chunkID, nodeCollection, (guid, collection) => nodeCollection);

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new InsertionTime(stopwatch.Elapsed));
            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Insertion: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            PendingMesh<int> pendingMesh = ChunkMesher.GenerateMesh(blocks, new INodeCollection<ushort>[6], false);
            _PendingMeshes.AddOrUpdate(chunkID, pendingMesh, (guid, mesh) => pendingMesh);

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));


            DiagnosticsProvider.Stopwatches.Return(stopwatch);
        }

        private bool _KeysPressed;

        private void DiagnosticsInputCheck()
        {
            if (!InputManager.Instance.IsKeyPressed(Key.ShiftLeft) || !InputManager.Instance.IsKeyPressed(Key.B))
            {
                _KeysPressed = false;
                return;
            }
            else if (_KeysPressed)
            {
                return;
            }

            _KeysPressed = true;


            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsSystem),
                $"Average generation times: {DiagnosticsProvider.GetGroup<ChunkGenerationDiagnosticGroup>()}"));
        }
    }
}
