#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using ConcurrentPools;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input.Common;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private readonly OrderedList<BuildStep> _BuildSteps;
        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _PendingBlockCollections;
        private readonly ConcurrentDictionary<Guid, PendingMesh<int>> _PendingMeshes;

        private bool _KeysPressed;

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
                if (!entity.TryGetComponent(out Chunk? chunk) || !entity.TryGetComponent(out Translation? translation)) continue;

                switch (chunk.State)
                {
                    case GenerationState.Ungenerated:
                        BoundedThreadPool.QueueWork(() => GenerateChunk(chunk.ID,
                            new BuildStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY, GenerationConstants.PERSISTENCE,
                                Vector3i.FromVector3(translation.Value)), _BuildSteps));

                        chunk.State += 1;
                        break;
                    case GenerationState.AwaitingBuilding when _PendingBlockCollections.TryRemove(chunk.ID, out INodeCollection<ushort>? blocks):
                        chunk.Blocks = blocks;
                        chunk.State += 1;
                        break;
                    case GenerationState.AwaitingMeshing when _PendingMeshes.TryRemove(chunk.ID, out PendingMesh<int>? pendingMesh):
                        Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
                        stopwatch.Restart();

                        if (!entity.TryGetComponent(out RenderMesh? renderMesh)) entityManager.RegisterComponent(entity, renderMesh = new RenderMesh());
                        if (renderMesh.Mesh is null or not Mesh<int>) renderMesh.Mesh = new Mesh<int>();

                        Mesh<int> mesh = (renderMesh.Mesh as Mesh<int>)!;
                        mesh.ModifyVertexAttributes<int>(0u, 0);
                        mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes, BufferDraw.DynamicDraw);
                        mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes, BufferDraw.DynamicDraw);

                        if (Shader.TryLoadShaderWithCache("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl", out Shader? shader))
                        {
                            if (entity.TryGetComponent(out RenderShader? renderShader))
                            {
                                if (renderShader.Value.ID != shader.ID) renderShader.Value = shader;
                            }
                            else entityManager.RegisterComponent(entity, new RenderShader(shader));
                        }
                        else Log.Error($"Failed to load a shader for chunk at {translation.Value}.");

                        stopwatch.Stop();

                        DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));

                        Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                            $"Applied mesh: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

                        DiagnosticsSystem.Stopwatches.Return(stopwatch);

                        chunk.State += 1;
                        break;
                }
            }

            DiagnosticsInputCheck();
        }

        private void GenerateChunk(Guid chunkID, BuildStep.Parameters parameters, IEnumerable<BuildStep> buildSteps)
        {
            static INodeCollection<ushort> GenerateNodeCollectionImpl(ref Span<ushort> blocks)
            {
                Octree<ushort> blocksCompressed = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, BlockRegistry.AirID);

                int index = 0;

                for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
                    blocksCompressed.SetPoint(x, y, z, blocks[index]);

                return blocksCompressed;
            }

            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            foreach (BuildStep generationStep in buildSteps) generationStep.Generate(parameters, blocks);

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new BuildingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Built: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            INodeCollection<ushort> nodeCollection = GenerateNodeCollectionImpl(ref blocks);
            if (!_PendingBlockCollections.TryAdd(chunkID, nodeCollection)) Log.Error($"Failed to add chunk({parameters.Origin}) blocks.");

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new InsertionTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Insertion: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            PendingMesh<int> pendingMesh = ChunkMesher.GeneratePackedMesh(blocks, new INodeCollection<ushort>[6], true);
            if (!_PendingMeshes.TryAdd(chunkID, pendingMesh)) Log.Error($"Failed to add chunk({parameters.Origin}) mesh.");

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));

            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

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
