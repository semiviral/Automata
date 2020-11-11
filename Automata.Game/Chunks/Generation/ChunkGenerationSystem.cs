#region

using System;
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
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input.Common;

#endregion


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
        private readonly ConcurrentChannel<(IEntity, PendingMesh<PackedVertex>)> _PendingMeshes;

        private bool _KeysPressed;

        public ChunkGenerationSystem()
        {
            _BuildSteps = new OrderedLinkedList<IGenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingBlocks = new ConcurrentChannel<(IEntity, Palette<Block>)>(true, false);
            _PendingMeshes = new ConcurrentChannel<(IEntity, PendingMesh<PackedVertex>)>(true, false);

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(Chunk))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            while (_PendingBlocks.TryTake(out (IEntity Entity, Palette<Block> Blocks) pendingBlocks)
                   && !pendingBlocks.Entity.Destroyed
                   && pendingBlocks.Entity.TryGetComponent(out Chunk? chunk))
            {
                chunk.Blocks = pendingBlocks.Blocks;
                chunk.State += 1;
            }

            while (_PendingMeshes.TryTake(out (IEntity Entity, PendingMesh<PackedVertex> Mesh) pendingMesh)
                   && !pendingMesh.Entity.Destroyed
                   && pendingMesh.Entity.TryGetComponent(out Chunk? chunk))
            {
                PrepareChunkForRendering(entityManager, pendingMesh.Entity, pendingMesh.Mesh);
                chunk.State += 1;
            }

            IGenerationStep.Parameters parameters = new IGenerationStep.Parameters(GenerationConstants.Seed)
            {
                Frequency = 0.008f
            };

            foreach ((IEntity entity, Chunk chunk, Translation translation) in entityManager.GetEntities<Chunk, Translation>())
            {
                switch (chunk.State)
                {
                    case GenerationState.Ungenerated:
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateBlocks(entity, Vector3i.FromVector3(translation.Value), parameters));
                        chunk.State += 1;
                        break;

                    case GenerationState.Unmeshed when chunk.IsStateLockstep(): // don't generate mesh until all neighbors are ready
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateMesh(entity, chunk, Vector3i.FromVector3(translation.Value)));
                        chunk.State += 1;
                        break;
                }
            }

            DiagnosticsInputCheck();
        }

        private async ValueTask GenerateBlocks(IEntity entity, Vector3i origin, IGenerationStep.Parameters parameters)
        {
            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            Palette<Block> GenerateTerrainAndBuildPalette()
            {
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

        private async ValueTask GenerateMesh(IEntity entity, Chunk chunk, Vector3i origin)
        {
            if (chunk.Blocks is null)
            {
                Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem), $"Cannot build chunk; it has no blocks ({origin})."));
                return;
            }

            Stopwatch stopwatch = DiagnosticsSystem.Stopwatches.Rent();
            stopwatch.Restart();

            PendingMesh<PackedVertex> pendingMesh = ChunkMesher.GeneratePackedMesh(chunk.Blocks, chunk.NeighborBlocks().ToArray());
            await _PendingMeshes.AddAsync((entity, pendingMesh)).ConfigureAwait(false);

            stopwatch.Stop();

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkGenerationSystem),
                $"Meshed: '{chunk.ID}' ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms, vertexes {pendingMesh.Vertexes.Length}, indexes {pendingMesh.Indexes.Length})"));

            DiagnosticsSystem.Stopwatches.Return(stopwatch);
        }

        private static void PrepareChunkForRendering(EntityManager entityManager, IEntity entity, PendingMesh<PackedVertex> pendingMesh)
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

        private static bool ApplyMesh(EntityManager entityManager, IEntity entity, PendingMesh<PackedVertex> pendingMesh)
        {
            bool hasRenderMesh = entity.TryGetComponent(out RenderMesh? renderMesh);

            if (pendingMesh.IsEmpty)
            {
                if (hasRenderMesh) renderMesh!.Mesh = null;
                return false;
            }

            if (!hasRenderMesh) entityManager.RegisterComponent(entity, renderMesh = new RenderMesh());
            if (renderMesh!.Mesh is null or not Mesh<PackedVertex>) renderMesh.Mesh = new Mesh<PackedVertex>();

            Mesh<PackedVertex> mesh = (renderMesh.Mesh as Mesh<PackedVertex>)!;

            if (!mesh.VertexArrayObject.VertexAttributes.SequenceEqual(_DefaultAttributes))
            {
                mesh.VertexArrayObject.AllocateVertexAttributes(_DefaultAttributes);
                mesh.VertexArrayObject.CommitVertexAttributes();
            }

            mesh.VertexesBuffer.SetBufferData(pendingMesh.Vertexes, BufferDraw.DynamicDraw);
            mesh.IndexesBuffer.SetBufferData(pendingMesh.Indexes, BufferDraw.DynamicDraw);
            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, IEntity entity)
        {
            ProgramPipeline programPipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryGetComponent(out Material? material))
            {
                if (material.Pipeline.Handle != programPipeline.Handle) material.Pipeline = programPipeline;
            }
            else entityManager.RegisterComponent(entity, material = new Material(programPipeline));

            material.Textures.Add(TextureAtlas.Instance.Blocks ?? throw new NullReferenceException("Blocks texture array not initialized."));
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
