using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Concurrency;
using Automata.Engine.Diagnostics;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.Meshes;
using Automata.Game.Blocks;
using Automata.Game.Chunks.Generation.Meshing;
using Automata.Game.Chunks.Generation.Structures;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input;

namespace Automata.Game.Chunks.Generation
{
    public class ChunkGenerationSystem : ComponentSystem
    {
        private readonly VoxelWorld _VoxelWorld;
        private readonly IOrderedCollection<IGenerationStep> _BuildSteps;
        private readonly ConcurrentChannel<(Entity, Chunk, NonAllocatingQuadsMeshData<uint, PackedVertex>)> _PendingMeshes;

        public ChunkGenerationSystem(VoxelWorld voxelWorld) : base(voxelWorld)
        {
            _VoxelWorld = voxelWorld;
            _BuildSteps = new OrderedLinkedList<IGenerationStep>();
            _BuildSteps.AddLast(new TerrainGenerationStep());
            _PendingMeshes = new ConcurrentChannel<(Entity, Chunk, NonAllocatingQuadsMeshData<uint, PackedVertex>)>(true, false);

            DiagnosticsProvider.EnableGroup<ChunkGenerationDiagnosticGroup>();
        }

        public override void Registered(EntityManager entityManager)
        {
            // prints average chunk generation times
            InputManager.Instance.RegisterInputAction(() =>
            {
                Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsPool),
                    $"Average generation times: {DiagnosticsProvider.GetGroup<ChunkGenerationDiagnosticGroup>()}"));
            }, Key.ShiftLeft, Key.B);

            // prints all chunk states
            InputManager.Instance.RegisterInputAction(() =>
            {
                IEnumerable<(GenerationState, int)> states = entityManager.GetComponents<Chunk>().Select(chunk => (chunk.State, chunk.TimesMeshed));

                Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(DiagnosticsPool),
                    $"{string.Join(", ", states)} ———{entityManager.GetComponentCount<Chunk>()} TOTAL CHUNKS———"));
            }, Key.ShiftLeft, Key.V);
        }

        [HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(Chunk))]
        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            // empty channel of any pending meshes, apply the meshes, and update the material
            while (_PendingMeshes.TryTake(out (Entity Entity, Chunk Chunk, NonAllocatingQuadsMeshData<uint, PackedVertex> Data) pendingMesh))
            {
                if (!pendingMesh.Entity.Disposed && pendingMesh.Chunk.State is GenerationState.GeneratingMesh)
                {
                    entityManager.RegisterComponent(pendingMesh.Entity, new AllocatedMeshData<uint, PackedVertex>(pendingMesh.Data));
                    pendingMesh.Chunk.State += 1;
                }
                else
                {
                    // if the mesh data isn't going to be used, ensure we dispose it
                    pendingMesh.Data.Dispose();
                }
            }

            // iterate over each valid chunk and process the generateable states
            foreach ((Entity entity, Chunk chunk, Transform transform) in entityManager.GetEntitiesWithComponents<Chunk, Transform>())
            {
                switch (chunk.State)
                {
                    case GenerationState.AwaitingTerrain:
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateBlocks(chunk, transform.Translation.Convert<int>(),
                            new IGenerationStep.Parameters(GenerationConstants.Seed, transform.Translation.Convert<int>().GetHashCode())
                            {
                                Frequency = 0.008f
                            }));

                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingStructures:
                        //BoundedInvocationPool.Instance.Enqueue(_ => GenerateStructures(chunk, Vector3<int>.FromVector3(transform.Translation)));
                        chunk.State += 1;
                        chunk.State += 1;
                        break;

                    case GenerationState.AwaitingMesh when chunk.Neighbors.All(neighbor => neighbor?.State is null or >= GenerationState.AwaitingMesh):
                        BoundedInvocationPool.Instance.Enqueue(_ => GenerateMesh(entity, chunk));
                        chunk.State += 1;
                        break;
                }
            }

            return ValueTask.CompletedTask;
        }


        #region Generation

        private Task GenerateBlocks(Chunk chunk, Vector3<int> origin, IGenerationStep.Parameters parameters)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();

            stopwatch.Restart();

            // block ids for generating
            Span<ushort> data = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];

            foreach (IGenerationStep generationStep in _BuildSteps)
            {
                generationStep.Generate(origin, parameters, data);
            }

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new BuildingTime(stopwatch.Elapsed));
            stopwatch.Restart();

            Palette<Block> palette = new Palette<Block>(GenerationConstants.CHUNK_SIZE_CUBED, new Block(BlockRegistry.AirID));

            for (int index = 0; index < GenerationConstants.CHUNK_SIZE_CUBED; index++)
            {
                palette[index] = new Block(data[index]);
            }

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new InsertionTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);

            chunk.Blocks = palette;
            chunk.State += 1;

            return Task.CompletedTask;
        }

        private async Task GenerateStructures(Chunk chunk, Vector3<int> origin)
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
                Vector3<int> offset = new Vector3<int>(x, y, z);

                if (!testStructure.CheckPlaceStructureAt(World, random, origin + offset))
                {
                    continue;
                }

                foreach ((Vector3<int> local, ushort blockID) in testStructure.StructureBlocks)
                {
                    Vector3<int> modificationOffset = offset + local;

                    // see if we can allocate the modification directly to the chunk
                    if (Vector.All(modificationOffset >= 0) && Vector.All(modificationOffset < GenerationConstants.CHUNK_SIZE))
                    {
                        await chunk.Modifications.AddAsync(new ChunkModification
                        {
                            BlockIndex = index,
                            BlockID = blockID
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        // if not, just go ahead and delegate the modification allocation to the world.
                        await _VoxelWorld.AllocateChunkModification(origin + modificationOffset, blockID).ConfigureAwait(false);
                    }
                }
            }

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new StructuresTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
            chunk.State += 1;
        }

        private async Task GenerateMesh(Entity entity, Chunk chunk)
        {
            if (chunk.Blocks is null)
            {
                return;
            }

            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            NonAllocatingQuadsMeshData<uint, PackedVertex> pendingQuads = ChunkMesher.GeneratePackedMesh(chunk.Blocks, chunk.NeighborBlocks().ToArray());
            await _PendingMeshes.AddAsync((entity, chunk, pendingQuads)).ConfigureAwait(false);

            DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new MeshingTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        #endregion
    }
}
