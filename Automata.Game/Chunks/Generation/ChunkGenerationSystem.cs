#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using ConcurrencyPools;
using DiagnosticsProviderNS;
using Generic_Octree;
using Serilog;
using Silk.NET.Input.Common;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private readonly OrderedLinkedList<GenerationStep> _BuildSteps;
        private readonly ConcurrentDictionary<Guid, INodeCollection<ushort>> _PendingBlockCollections;
        private readonly ConcurrentDictionary<Guid, PendingMesh<int>> _PendingMeshes;

        private bool _KeysPressed;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedLinkedList<GenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingBlockCollections = new ConcurrentDictionary<Guid, INodeCollection<ushort>>();
            _PendingMeshes = new ConcurrentDictionary<Guid, PendingMesh<int>>();

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<Chunk, Translation>())
            {
                if (!entity.TryGetComponent(out Chunk? chunk) || !entity.TryGetComponent(out Translation? translation))
                {
                    continue;
                }

                switch (chunk.State)
                {
                    case GenerationState.Ungenerated when chunk.MinimalNeighborState() >= GenerationState.Ungenerated:
                        BoundedPool.Active.QueueWork(() => GenerateBlocks(chunk, Vector3i.FromVector3(translation.Value),
                            new GenerationStep.Parameters(GenerationConstants.Seed, GenerationConstants.FREQUENCY, GenerationConstants.PERSISTENCE)));

                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingBuilding when _PendingBlockCollections.TryRemove(chunk.ID, out INodeCollection<ushort>? blocks):
                        chunk.Blocks = blocks;
                        chunk.State += 1;
                        break;

                    case GenerationState.Unmeshed when chunk.MinimalNeighborState() >= GenerationState.Unmeshed:
                        BoundedPool.Active.QueueWork(() => GenerateMesh(chunk, Vector3i.FromVector3(translation.Value)));

                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingMeshing when _PendingMeshes.TryRemove(chunk.ID, out PendingMesh<int>? pendingMesh):
                        ApplyMesh(entityManager, entity, chunk.ID, pendingMesh);

                        chunk.State += 1;
                        break;
                }
            }

            DiagnosticsInputCheck();
        }

        private void GenerateBlocks(Chunk chunk, Vector3i origin, GenerationStep.Parameters parameters)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            foreach (GenerationStep generationStep in _BuildSteps)
            {
                generationStep.Generate(origin, parameters, blocks);
            }

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new BuildingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Built: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            stopwatch.Restart();

            INodeCollection<ushort> nodeCollection = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, BlockRegistry.AirID);

            int index = 0;

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                nodeCollection.SetPoint(x, y, z, blocks[index]);
            }

            if (!_PendingBlockCollections.TryAdd(chunk.ID, nodeCollection))
            {
                Log.Error($"Failed to add chunk({origin}) blocks.");
            }

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new InsertionTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Insertion: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));

            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private void GenerateMesh(Chunk chunk, Vector3i origin)
        {
            if (chunk.Blocks is null)
            {
                Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                    $"Attempted to mesh chunk {origin}, but it has not generated blocks."));

                return;
            }

            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            int index = 0;
            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                blocks[index] = chunk.Blocks.GetPoint(x, y, z);
            }

            PendingMesh<int> pendingMesh = ChunkMesher.GeneratePackedMesh(blocks, chunk.NeighborBlocks());

            if (!_PendingMeshes.TryAdd(chunk.ID, pendingMesh))
            {
                Log.Error($"Failed to add chunk({origin}) mesh.");
            }

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));

            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private static void ApplyMesh(EntityManager entityManager, IEntity entity, Guid chunkID, PendingMesh<int> pendingMesh)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            if (!entity.TryGetComponent(out RenderMesh? renderMesh))
            {
                entityManager.RegisterComponent(entity, renderMesh = new RenderMesh());
            }

            if (renderMesh.Mesh is null or not Mesh<int>)
            {
                renderMesh.Mesh = new Mesh<int>(sizeof(int) + sizeof(int));
            }

            Mesh<int> mesh = (renderMesh.Mesh as Mesh<int>)!;
            mesh.ModifyVertexAttributes<int>(0u, 0);
            mesh.ModifyVertexAttributes<int>(1u, 1);
            mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes, BufferDraw.DynamicDraw);
            mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes, BufferDraw.DynamicDraw);

            if (Shader.TryLoadShaderWithCache("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl", out Shader? shader))
            {
                if (entity.TryGetComponent(out Material? material))
                {
                    if (material.Shader.ID != shader.ID)
                    {
                        material.Shader = shader;
                    }
                }
                else
                {
                    entityManager.RegisterComponent(entity, material = new Material(shader));
                }

                material.Textures[0] = TextureAtlas.Instance.Blocks;
            }
            else
            {
                Log.Error($"Failed to load a shader for chunk at {entity.GetComponent<Translation>().Value}.");
            }

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
            DiagnosticsSystem.Stopwatches.Return(stopwatch);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Applied mesh: '{chunkID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));
        }

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
