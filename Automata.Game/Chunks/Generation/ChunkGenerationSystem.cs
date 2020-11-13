using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using Automata.Game.Chunks.Generation.Meshing;
using Automata.Game.Chunks.Generation.Structures;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input.Common;

namespace Automata.Game.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private static readonly IVertexAttribute[] _DefaultAttributes =
        {
            new VertexAttribute<int>(0u, 1u, 0u),
            new VertexAttribute<int>(1u, 1u, sizeof(int))
        };

        private readonly IOrderedCollection<IGenerationStep> _BuildSteps;
        private readonly ConcurrentChannel<(IEntity, Palette<Block>)> _PendingBlocks;
        private readonly ConcurrentChannel<(IEntity, NonAllocatingMeshData<PackedVertex>)> _PendingMeshes;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedLinkedList<IGenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingBlocks = new ConcurrentChannel<(IEntity, Palette<Block>)>(true, false);
            _PendingMeshes = new ConcurrentChannel<(IEntity, NonAllocatingMeshData<PackedVertex>)>(true, false);

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        public override void Registered(EntityManager entityManager)
        {
            // prints average chunk generation times
            InputManager.Instance.InputActions.Add(new InputAction(() =>
            {
                Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsSystem),
                    $"Average generation times: {DiagnosticsProvider.GetGroup<ChunkGenerationDiagnosticGroup>()}"));
            }, Key.ShiftLeft, Key.B));

            // prints all chunk states
            InputManager.Instance.InputActions.Add(new InputAction(() =>
            {
                IEnumerable<GenerationState> states = entityManager.GetComponents<Chunk>().Select(chunk => chunk.State);
                Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsSystem), string.Join(", ", states)));
            }, Key.ShiftLeft, Key.V));
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            // empty channel of any pending blocks
            while (_PendingBlocks.TryTake(out (IEntity Entity, Palette<Block> Blocks) pendingBlocks)
                   && !pendingBlocks.Entity.Destroyed
                   && pendingBlocks.Entity.TryFind(out Chunk? chunk))
            {
                Debug.Assert(chunk.State is GenerationState.GeneratingTerrain);

                chunk.Blocks = pendingBlocks.Blocks;
                chunk.State += 1;
            }

            // empty channel of any pending meshes, apply the meshes, and update the material
            while (_PendingMeshes.TryTake(out (IEntity Entity, NonAllocatingMeshData<PackedVertex> Data) pendingMesh)
                   && !pendingMesh.Entity.Destroyed
                   && pendingMesh.Entity.TryFind(out Chunk? chunk))
            {
                Debug.Assert(chunk.State is GenerationState.GeneratingMesh);

                PrepareChunkForRendering(entityManager, pendingMesh.Entity, pendingMesh.Data);
                pendingMesh.Data.Dispose();
                chunk.State += 1;
            }

            // iterate over each valid chunk and process the generateable states
            foreach ((IEntity entity, Chunk chunk, Translation translation) in entityManager.GetEntitiesWithComponents<Chunk, Translation>())
                switch (chunk.State)
                {
                    case GenerationState.AwaitingTerrain:
                        Vector3i origin = Vector3i.FromVector3(translation.Value);

                        IGenerationStep.Parameters parameters = new IGenerationStep.Parameters(GenerationConstants.Seed, origin.GetHashCode())
                        {
                            Frequency = 0.008f
                        };

                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateBlocks(entity, origin, parameters));

                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingStructures:
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateStructures(chunk, Vector3i.FromVector3(translation.Value)));
                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingMesh when chunk.NeighborState(GenerationState.AwaitingMesh, ComparisonMode.EqualOrGreaterThan):
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateMesh(entity, chunk));
                        chunk.State += 1;
                        break;
                }

            return ValueTask.CompletedTask;
        }

        private async Task GenerateBlocks(IEntity entity, Vector3i origin, IGenerationStep.Parameters parameters)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();

            Palette<Block> GenerateTerrainAndBuildPalette()
            {
                stopwatch.Restart();

                // block ids for generating
                Span<ushort> data = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

                foreach (IGenerationStep generationStep in _BuildSteps) generationStep.Generate(origin, parameters, data);

                DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new BuildingTime(stopwatch.Elapsed));
                stopwatch.Restart();

                Palette<Block> palette = new Palette<Block>(GenerationConstants.CHUNK_SIZE_CUBED, new Block(BlockRegistry.AirID));
                for (int index = 0; index < GenerationConstants.CHUNK_SIZE_CUBED; index++) palette[index] = new Block(data[index]);

                DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new InsertionTime(stopwatch.Elapsed));
                return palette;
            }

            Palette<Block> blocks = GenerateTerrainAndBuildPalette();
            await _PendingBlocks.AddAsync((entity, blocks)).ConfigureAwait(false);
            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private async Task GenerateStructures(Chunk chunk, Vector3i origin)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();

            if (chunk.Blocks is null)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem), "Chunk has no blocks."));
                return;
            }

            IStructure testStructure = new TestStructure();
            Random random = new Random(origin.GetHashCode());

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++)
                if (testStructure.CheckPlaceStructureAt(random, origin))
                {
                    Vector3i offset = new Vector3i(x, y, z);

                    foreach ((Vector3i local, ushort blockID) in testStructure.StructureBlocks)
                    {
                        Vector3i modificationOffset = offset + local;

                        // see if we can allocate the modification directly to the chunk
                        if (Vector3b.All(modificationOffset >= 0) && Vector3b.All(modificationOffset < GenerationConstants.CHUNK_SIZE))
                            await chunk.Modifications.AddAsync(new ChunkModification
                            {
                                Local = modificationOffset,
                                BlockID = blockID
                            }).ConfigureAwait(false);

                        // if not, just go ahead and delegate the modification to the world.
                        else await (_CurrentWorld as VoxelWorld)!.Chunks.AllocateChunkModification(origin + modificationOffset, blockID).ConfigureAwait(false);
                    }
                }

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            DiagnosticsSystem.Stopwatches.Return(stopwatch);
            chunk.State += 1;
        }

        private async Task GenerateMesh(IEntity entity, Chunk chunk)
        {
            if (chunk.Blocks is null)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem), "Chunk has no blocks."));
                return;
            }

            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            NonAllocatingMeshData<PackedVertex> pendingMesh = ChunkMesher.GeneratePackedMeshData(chunk.Blocks, chunk.NeighborBlocks.ToArray());
            await _PendingMeshes.AddAsync((entity, pendingMesh)).ConfigureAwait(false);

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private static void PrepareChunkForRendering(EntityManager entityManager, IEntity entity, NonAllocatingMeshData<PackedVertex> pendingMesh)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            if (!ApplyMesh(entityManager, entity, pendingMesh))
            {
                DiagnosticsSystem.Stopwatches.Return(stopwatch);
                return;
            }

            ConfigureMaterial(entityManager, entity);

            stopwatch.Stop();
            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private static bool ApplyMesh(EntityManager entityManager, IEntity entity, NonAllocatingMeshData<PackedVertex> pendingMesh)
        {
            bool hasRenderMesh = entity.TryFind(out RenderMesh? renderMesh);

            if (pendingMesh.IsEmpty)
            {
                if (hasRenderMesh) entityManager.RemoveComponent<RenderMesh>(entity);

                return false;
            }

            if (!hasRenderMesh) entityManager.RegisterComponent(entity, renderMesh = new RenderMesh());
            if (renderMesh!.Mesh is null or not Mesh<PackedVertex>) renderMesh.Mesh = new Mesh<PackedVertex>(GLAPI.Instance.GL);

            Mesh<PackedVertex> mesh = (renderMesh.Mesh as Mesh<PackedVertex>)!;

            if (!mesh.VertexArrayObject.VertexAttributes.SequenceEqual(_DefaultAttributes))
            {
                mesh.VertexArrayObject.AllocateVertexAttributes(_DefaultAttributes);
                mesh.VertexArrayObject.CommitVertexAttributes();
            }

            mesh.VertexesBufferObject.SetBufferData(pendingMesh.Vertexes.Segment.Span, BufferDraw.DynamicDraw);
            mesh.IndexesBufferObject.SetBufferData(pendingMesh.Indexes.Segment.Span, BufferDraw.DynamicDraw);
            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, IEntity entity)
        {
            ProgramPipeline programPipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryFind(out Material? material))
            {
                if (material.Pipeline.Handle != programPipeline.Handle) material.Pipeline = programPipeline;
            }
            else entityManager.RegisterComponent(entity, material = new Material(programPipeline));

            material.Textures.Add(TextureAtlas.Instance.Blocks ?? throw new NullReferenceException("Blocks texture array not initialized."));
        }
    }
}
