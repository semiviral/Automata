#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Extensions;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using ConcurrentPools;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Game.Chunks.Generation
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
        }

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Chunk, Translation>())
            {
                Chunk chunk = entity.GetComponent<Chunk>();

                switch (chunk.State)
                {
                    case GenerationState.Ungenerated:
                        Translation translation = entity.GetComponent<Translation>();

                        BuildStep.Parameters parameters = new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY,
                            GenerationConstants.PERSISTENCE, Vector3i.FromVector3(translation.Value));

                        BoundedThreadPool.QueueWork(() => GenerateChunk(chunk.ID, parameters));

                        chunk.State = chunk.State.Next();
                        break;
                    case GenerationState.AwaitingBuilding when _PendingBlockCollections.TryRemove(chunk.ID, out INodeCollection<ushort>? blocks):
                        chunk.Blocks = blocks;
                        chunk.State = chunk.State.Next();
                        break;
                    case GenerationState.AwaitingMeshing when _PendingMeshes.TryRemove(chunk.ID, out PendingMesh<int>? pendingMesh):
                        Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
                        stopwatch.Restart();

                        Mesh<int> mesh = new Mesh<int>();
                        mesh.VertexArrayObject.VertexAttributeIPointer(0u, 1, VertexAttribPointerType.Int, 0);
                        mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes);
                        mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes);

                        if (entity.TryGetComponent(out RenderMesh? renderMesh)) renderMesh.Mesh = mesh;
                        else entityManager.RegisterComponent(entity, renderMesh = new RenderMesh(mesh));

                        if (entity.TryGetComponent(out Scale? modelScale)
                            | entity.TryGetComponent(out Rotation? modelRotation)
                            | entity.TryGetComponent(out Translation? modelTranslation))
                        {
                            renderMesh.Model = Matrix4x4.Identity;
                            renderMesh.Model *= Matrix4x4.CreateScale(modelScale?.Value ?? Scale.DEFAULT);
                            renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation?.Value ?? Quaternion.Identity);
                            renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation?.Value ?? Vector3.Zero);
                        }

                        stopwatch.Stop();

                        DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));

                        Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                            $"Applied mesh: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

                        DiagnosticsSystem.Stopwatches.Return(stopwatch);

                        chunk.State = chunk.State.Next();
                        break;
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
                    blocksCompressed.SetPoint(x, y, z, blocks[index]);

                return blocksCompressed;
            }

            if (parameters is null || _BuildSteps is null) throw new InvalidOperationException("Job data has not been provided.");

            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            foreach (BuildStep generationStep in _BuildSteps) generationStep.Generate(parameters, blocks);

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

            PendingMesh<int> pendingMesh = ChunkMesher.GenerateMesh(blocks, new INodeCollection<ushort>[6], true);
            _PendingMeshes.AddOrUpdate(chunkID, pendingMesh, (guid, mesh) => pendingMesh);

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));

            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private bool _KeysPressed;

        private void DiagnosticsInputCheck()
        {
            if (!InputManager.Instance.IsKeyPressed(Key.ShiftLeft) || !InputManager.Instance.IsKeyPressed(Key.B))
            {
                _KeysPressed = false;
                return;
            }
            else if (_KeysPressed) return;

            _KeysPressed = true;

            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsSystem),
                $"Average generation times: {DiagnosticsProvider.GetGroup<ChunkGenerationDiagnosticGroup>()}"));
        }
    }
}
