using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering;
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
        private readonly ConcurrentChannel<(IEntity, NonAllocatingList<Quad<PackedVertex>>)> _PendingMeshes;

        private MultiDrawIndirectMesh? _MultiDrawIndirectMesh;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedLinkedList<IGenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingMeshes = new ConcurrentChannel<(IEntity, NonAllocatingList<Quad<PackedVertex>>)>(true, false);

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        public override void Registered(EntityManager entityManager)
        {
            // prints average chunk generation times
            InputManager.Instance.InputActions.Add(new InputAction(() =>
            {
                Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsPool),
                    $"Average generation times: {DiagnosticsProvider.GetGroup<ChunkGenerationDiagnosticGroup>()}"));
            }, Key.ShiftLeft, Key.B));

            // prints all chunk states
            InputManager.Instance.InputActions.Add(new InputAction(() =>
            {
                IEnumerable<(GenerationState, int)> states = entityManager.GetComponents<Chunk>().Select(chunk => (chunk.State, chunk.TimesMeshed));
                Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsPool), string.Join(", ", states)));
            }, Key.ShiftLeft, Key.V));

            const uint one_kb = 1024u;
            const uint one_mb = one_kb * one_kb;
            const uint one_gb = one_kb * one_kb * one_kb;

            // _MultiDrawIndirectMesh = new MultiDrawIndirectMesh(GLAPI.Instance.GL, 3u * one_mb, one_gb);
            //
            // _MultiDrawIndirectMesh.VertexArrayObject.AllocateVertexAttributes(new IVertexAttribute[]
            // {
            //     new VertexAttribute<float>(3u + 0u, 4u, (uint)Marshal.OffsetOf<float>(nameof(Matrix4x4.M11)), 1u),
            //     new VertexAttribute<float>(3u + 1u, 4u, (uint)Marshal.OffsetOf<float>(nameof(Matrix4x4.M11)), 1u),
            //     new VertexAttribute<float>(3u + 2u, 4u, (uint)Marshal.OffsetOf<float>(nameof(Matrix4x4.M11)), 1u),
            //     new VertexAttribute<float>(3u + 3u, 4u, (uint)Marshal.OffsetOf<float>(nameof(Matrix4x4.M11)), 1u)
            // });
            //
            // entityManager.RegisterEntity(new Entity
            // {
            //     new RenderMesh
            //     {
            //         Mesh = _MultiDrawIndirectMesh
            //     },
            //     new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"))
            // });
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk)),
         HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderMesh)),
         HandledComponents(DistinctionStrategy.All, typeof(Chunk), typeof(RenderModel))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            // empty channel of any pending meshes, apply the meshes, and update the material
            while (_PendingMeshes.TryTake(out (IEntity Entity, NonAllocatingList<Quad<PackedVertex>> Quads) pendingMesh)
                   && !pendingMesh.Entity.Disposed
                   && pendingMesh.Entity.TryFind(out Chunk? chunk))
            {
                Debug.Assert(chunk.State is GenerationState.GeneratingMesh);

                PrepareChunkForRendering(entityManager, pendingMesh.Entity, pendingMesh.Quads);
                pendingMesh.Quads.Dispose();
                chunk.TimesMeshed += 1;
                chunk.State += 1;
            }

            // iterate over each valid chunk and process the generateable states
            foreach ((IEntity entity, Chunk chunk, Translation translation) in entityManager.GetEntitiesWithComponents<Chunk, Translation>())
                switch (chunk.State)
                {
                    case GenerationState.AwaitingTerrain:
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateBlocks(chunk, Vector3i.FromVector3(translation.Value),
                            new IGenerationStep.Parameters(GenerationConstants.Seed, Vector3i.FromVector3(translation.Value).GetHashCode())
                            {
                                Frequency = 0.008f
                            }));

                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingStructures:
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateStructures(chunk, Vector3i.FromVector3(translation.Value)));
                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingMesh when chunk.Neighbors.All(neighbor => neighbor?.State is null or >= GenerationState.AwaitingMesh):
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateMesh(entity, chunk));
                        chunk.State += 1;
                        break;
                }

            return ValueTask.CompletedTask;
        }

        private Task GenerateBlocks(Chunk chunk, Vector3i origin, IGenerationStep.Parameters parameters)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();

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

            chunk.Blocks = GenerateTerrainAndBuildPalette();
            chunk.State += 1;

            DiagnosticsPool.Stopwatches.Return(stopwatch);
            return Task.CompletedTask;
        }

        private async Task GenerateStructures(Chunk chunk, Vector3i origin)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();

            if (chunk.Blocks is null)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem), "Chunk has no blocks."));
                return;
            }

            IStructure testStructure = new TestStructure();
            Random random = new Random(origin.GetHashCode());

            for (int y = 0, index = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
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
                                BlockIndex = index,
                                BlockID = blockID
                            }).ConfigureAwait(false);

                        // if not, just go ahead and delegate the modification to the world.
                        else await (_CurrentWorld as VoxelWorld)!.Chunks.AllocateChunkModification(origin + modificationOffset, blockID).ConfigureAwait(false);
                    }
                }

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new StructuresTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
            chunk.State += 1;
        }

        private async Task GenerateMesh(IEntity entity, Chunk chunk)
        {
            if (chunk.Blocks is null)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem), "Chunk has no blocks."));
                return;
            }

            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            NonAllocatingList<Quad<PackedVertex>> pendingQuads = ChunkMesher.GeneratePackedMesh(chunk.Blocks, chunk.NeighborBlocks.ToArray());
            await _PendingMeshes.AddAsync((entity, pendingQuads)).ConfigureAwait(false);

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private static void PrepareChunkForRendering(EntityManager entityManager, IEntity entity, NonAllocatingList<Quad<PackedVertex>> pendingQuads)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            if (ApplyMesh(entityManager, entity, pendingQuads))
            {
                ConfigureMaterial(entityManager, entity);
                DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
            }

            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private static bool ApplyMesh(EntityManager entityManager, IEntity entity, NonAllocatingList<Quad<PackedVertex>> pendingQuads)
        {
            bool hasRenderMesh = entity.TryFind(out RenderMesh? renderMesh);

            if (pendingQuads.IsEmpty)
            {
                if (hasRenderMesh) entityManager.RemoveComponent<RenderMesh>(entity);

                return false;
            }

            if (!hasRenderMesh) entityManager.RegisterComponent(entity, renderMesh = new RenderMesh());
            if (renderMesh!.Mesh is null or not QuadsMesh<PackedVertex>) renderMesh.Mesh = new QuadsMesh<PackedVertex>(GLAPI.Instance.GL);

            QuadsMesh<PackedVertex> quadsMesh = (renderMesh.Mesh as QuadsMesh<PackedVertex>)!;

            if (!quadsMesh!.VertexArrayObject.VertexAttributes.SequenceEqual(_DefaultAttributes))
            {
                quadsMesh.VertexArrayObject.AllocateVertexAttributes(_DefaultAttributes);
                quadsMesh.VertexArrayObject.CommitVertexAttributes();
            }

            quadsMesh.BufferObject.SetBufferData(pendingQuads.Segment, BufferDraw.DynamicDraw);
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
