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
using Automata.Engine.Extensions;
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
        private readonly IOrderedCollection<IGenerationStep> _BuildSteps;
        private readonly ConcurrentChannel<(IEntity, Chunk, NonAllocatingQuadsMeshData<PackedVertex>)> _PendingMeshes;

        private MultiDrawIndirectMesh? _MultiDrawIndirectMesh;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedLinkedList<IGenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingMeshes = new ConcurrentChannel<(IEntity, Chunk, NonAllocatingQuadsMeshData<PackedVertex>)>(true, false);

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        public override unsafe void Registered(EntityManager entityManager)
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

            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh(GLAPI.Instance.GL, 1_000_000, 1_000_000_000);

            _MultiDrawIndirectMesh.AllocateVertexAttributes(true,
                new VertexAttribute<int>(0u, 1u, 0u),
                new VertexAttribute<int>(1u, 1u, 4u),
                new VertexAttribute<float>(2u + 0u, 4u, (uint)sizeof(Vector4) * 0u, 1u));

            _MultiDrawIndirectMesh.FinalizeVertexArrayObject(0);

            Material material = new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"));
            material.Textures.Add(TextureAtlas.Instance.Blocks!);

            entityManager.CreateEntity(
                material,
                new RenderMesh
                {
                    Mesh = _MultiDrawIndirectMesh
                });
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            // empty channel of any pending meshes, apply the meshes, and update the material
            while (_PendingMeshes.TryTake(out (IEntity Entity, Chunk Chunk, NonAllocatingQuadsMeshData<PackedVertex> Data) pendingMesh))
            {
                if (!pendingMesh.Entity.Disposed)
                {
                    Debug.Assert(pendingMesh.Chunk.State is GenerationState.GeneratingMesh);
                    PrepareChunkForRendering(entityManager, pendingMesh.Entity, pendingMesh.Data);
                }

                // we ALWAYS update chunk state, so we can properly dispose of it
                // and be conscious of not doing so when its generating
                pendingMesh.Chunk.TimesMeshed += 1;
                pendingMesh.Chunk.State += 1;
                pendingMesh.Data.Dispose();
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
                        //BoundedInvocationPool.Instance.Enqueue(_ => GenerateStructures(chunk, Vector3i.FromVector3(translation.Value)));
                        chunk.State += 2;
                        break;

                    case GenerationState.AwaitingMesh when chunk.Neighbors.All(neighbor => neighbor?.State is null or >= GenerationState.AwaitingMesh):
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateMesh(entity, chunk));
                        chunk.State += 1;
                        break;
                }

            return ValueTask.CompletedTask;
        }


        #region Generation

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

            IStructure testStructure = new TreeStructure();
            Random random = new Random(origin.GetHashCode());

            for (int y = 0, index = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                Vector3i offset = new Vector3i(x, y, z);

                if (_CurrentWorld is null || !testStructure.CheckPlaceStructureAt(_CurrentWorld, random, origin + offset)) continue;

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

                    // if not, just go ahead and delegate the modification allocation to the world.
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

            NonAllocatingQuadsMeshData<PackedVertex> pendingQuads = ChunkMesher.GeneratePackedMesh(chunk.Blocks, chunk.NeighborBlocks().ToArray());
            await _PendingMeshes.AddAsync((entity, chunk, pendingQuads)).ConfigureAwait(false);

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        #endregion


        #region Render Prep

        private void PrepareChunkForRendering(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<PackedVertex> pendingData)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            if (ApplyMeshMultiDraw(entityManager, entity, pendingData))
            {
                ConfigureMaterial(entityManager, entity);
                DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
            }

            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private unsafe bool ApplyMeshMultiDraw(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<PackedVertex> pendingData)
        {
            if (_MultiDrawIndirectMesh is null) throw new NullReferenceException("Mesh is null!");

            _MultiDrawIndirectMesh.Visible = true;

            if (entity.TryFind(out DrawIndirectAllocation? drawIndirectAllocation)) entityManager.RemoveComponent<DrawIndirectAllocation>(entity);

            if (pendingData.IsEmpty) return false;

            int indexesLength = pendingData.Indexes.Count * sizeof(QuadIndexes);
            int totalLength = indexesLength + sizeof(Matrix4x4) + (pendingData.Vertexes.Count * sizeof(QuadVertexes<PackedVertex>));

            drawIndirectAllocation = new DrawIndirectAllocation(_MultiDrawIndirectMesh.RentMemory<byte>(totalLength, (uint)sizeof(uint), out nuint index),
                _MultiDrawIndirectMesh.RentCommand(out _));

            Span<byte> bufferMemory = drawIndirectAllocation.MemoryOwner.Memory.Span;

            drawIndirectAllocation.CommandOwner.Command =
                new DrawElementsIndirectCommand((uint)(pendingData.Vertexes.Count * 4), 1u, 0u, (uint)index, 0u);

            _MultiDrawIndirectMesh.WaitForBufferFreeSync();
            MemoryMarshal.AsBytes(pendingData.Indexes.Segment).CopyTo(bufferMemory);
            Vector4.One.Unroll<Vector4, byte>().CopyTo(bufferMemory.Slice(indexesLength));
            MemoryMarshal.AsBytes(pendingData.Vertexes.Segment).CopyTo(bufferMemory.Slice(indexesLength));

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

        #endregion
    }
}
