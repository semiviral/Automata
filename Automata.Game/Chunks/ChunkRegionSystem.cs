using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Game.Chunks.Generation;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input;
using Vector = Automata.Engine.Numerics.Vector;

namespace Automata.Game.Chunks
{
    public class ChunkRegionSystem : ComponentSystem
    {
        private static readonly OcclusionBounds _ChunkOcclusionBounds = new OcclusionBounds
        {
            Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS),
            Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE))
        };

        private readonly VoxelWorld _VoxelWorld;
        private readonly Ring<Queue<Chunk>> _ChunksRequiringRemesh;
        private readonly Ring<Stack<Chunk>> _ChunksPendingDisposal;

        public ChunkRegionSystem(VoxelWorld voxelWorld) : base(voxelWorld)
        {
            _VoxelWorld = voxelWorld;
            _ChunksRequiringRemesh = new Ring<Queue<Chunk>>(2, () => new Queue<Chunk>());
            _ChunksPendingDisposal = new Ring<Stack<Chunk>>(2, () => new Stack<Chunk>());
        }

        public override void Registered(EntityManager entityManager)
        {
            DiagnosticsProvider.EnableGroup<ChunkRegionLoadingDiagnosticGroup>();

            InputManager.Instance.RegisterInputAction(() => Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionSystem),
                $"Average update time: {DiagnosticsProvider.GetGroup<ChunkRegionLoadingDiagnosticGroup>().Average():0.00}ms")), Key.ShiftLeft, Key.X);
        }

        [HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(ChunkLoader))]
        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            RemeshChunksWithValidRemeshingState();
            DisposeChunksWithValidDisposalState();

            // determine whether any chunk loaders have moved out far enough to recalculate their loaded chunk region
            if (UpdateChunkLoaders(entityManager))
            {
                await RecalculateLoadedRegions(entityManager);
                _VoxelWorld.TrimMemory();
            }
        }

        private void RemeshChunksWithValidRemeshingState()
        {
            while (_ChunksRequiringRemesh.Current.TryDequeue(out Chunk? chunk))
            {
                switch (chunk!.State)
                {
                    case GenerationState.AwaitingMesh:
                    case GenerationState.Finished:
                        chunk.State = GenerationState.AwaitingMesh;
                        break;
                    default:
                        _ChunksRequiringRemesh.Next.Enqueue(chunk);
                        break;
                }
            }

            _ChunksRequiringRemesh.Increment();
        }

        private void DisposeChunksWithValidDisposalState()
        {
            while (_ChunksPendingDisposal.Current.TryPop(out Chunk? chunk))
            {
                if (!chunk!.IsGenerating)
                {
                    int index = 0;

                    for (; index < chunk!.Neighbors.Length; index++)
                    {
                        if (chunk!.Neighbors[index]?.IsGenerating is true)
                        {
                            break;
                        }
                    }

                    // this means we've successfully iterated every neighbor without a break
                    if (index == chunk!.Neighbors.Length)
                    {
                        chunk.RegionDispose();
                        continue;
                    }
                }

                _ChunksPendingDisposal.Next.Push(chunk);
            }

            _ChunksPendingDisposal.Increment();
        }

        private static bool UpdateChunkLoaders(EntityManager entityManager)
        {
            bool updated_chunk_positions = false;

            foreach ((Transform transform, ChunkLoader chunk_loader) in entityManager.GetComponents<Transform, ChunkLoader>())
            {
                Vector3<int> difference = Vector3<int>.Abs(transform.Translation.Convert<int>() - chunk_loader.Origin).WithY(0);

                if (!chunk_loader.RadiusChanged && Vector.All(difference < GenerationConstants.CHUNK_SIZE))
                {
                    continue;
                }

                chunk_loader.Origin = Vector3<int>.RoundBy(transform.Translation.Convert<int>(), GenerationConstants.CHUNK_SIZE);
                chunk_loader.RadiusChanged = false;
                updated_chunk_positions = true;
            }

            return updated_chunk_positions;
        }

        private async ValueTask RecalculateLoadedRegions(EntityManager entityManager)
        {
            using NonAllocatingList<ChunkLoader> loaders = GetChunkLoaders(entityManager);
            DeallocateChunksOutsideLoaderRadii(loaders, entityManager);
            await AllocateChunksWithinLoaderRadii(loaders, entityManager);
            UpdateRegionState();
        }

        private static NonAllocatingList<ChunkLoader> GetChunkLoaders(EntityManager entityManager)
        {
            int chunk_loader_count = (int)entityManager.GetComponentCount<ChunkLoader>();
            NonAllocatingList<ChunkLoader> loaders = new NonAllocatingList<ChunkLoader>(chunk_loader_count);

            foreach (ChunkLoader loader in entityManager.GetComponents<ChunkLoader>())
            {
                loaders.Add(loader);
            }

            return loaders;
        }

        private void DeallocateChunksOutsideLoaderRadii(NonAllocatingList<ChunkLoader> loaders, EntityManager entityManager)
        {
            foreach ((Vector3<int> origin, Chunk chunk) in _VoxelWorld)
            {
                bool disposable = true;

                foreach (ChunkLoader loader in loaders)
                {
                    if (loader.IsWithinRadius(origin))
                    {
                        disposable = false;
                    }
                }

                if (disposable)
                {
                    // todo it would be nice to just dispose of chunks outright, instead of deferring it
                    _ChunksPendingDisposal.Current.Push(chunk);
                    _VoxelWorld.DeallocateChunk(origin);
                    entityManager.RemoveEntity(chunk.Entity);
                }
            }
        }

        private async ValueTask AllocateChunksWithinLoaderRadii(NonAllocatingList<ChunkLoader> loaders, EntityManager entityManager)
        {
            foreach (ChunkLoader chunk_loader in loaders)
            {
                Vector3<int> y_adjusted_origin = chunk_loader.Origin.WithY(0);

                for (int z = -chunk_loader.Radius; z < (chunk_loader.Radius + 1); z++)
                for (int x = -chunk_loader.Radius; x < (chunk_loader.Radius + 1); x++)
                {
                    int x_pos = x * GenerationConstants.CHUNK_SIZE;
                    int z_pos = z * GenerationConstants.CHUNK_SIZE;

                    // remark: this relies on GenerationConstants.WORLD_HEIGHT_IN_CHUNKS being 8
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 0, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 1, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 2, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 3, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 4, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 5, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 6, z_pos));
                    await AllocateChunkEntity(entityManager, y_adjusted_origin + new Vector3<int>(x_pos, GenerationConstants.CHUNK_SIZE * 7, z_pos));
                }
            }
        }

        private async ValueTask AllocateChunkEntity(EntityManager entityManager, Vector3<int> origin)
        {
            Chunk? chunk = await _VoxelWorld.AllocateChunk(origin);

            if (chunk is not null)
            {
                Entity entity = entityManager.CreateEntity(
                    new Transform
                    {
                        Translation = origin.Convert<float>()
                    },
                    _ChunkOcclusionBounds,
                    chunk);

                chunk.Entity = entity;
            }
        }

        private void UpdateRegionState()
        {
            foreach ((Vector3<int> origin, Chunk chunk) in _VoxelWorld)
            {
                // here we assign this chunk's neighbors
                //
                // in addition, if this chunk is inactive (i.e. a new allocation) then
                // we also queue each neighbor for a remesh to ensure their meshes are
                // up-to-date with the new world shape.
                AssignNeighbors(origin, chunk!.Neighbors);

                if (chunk.State is GenerationState.Inactive)
                {
                    foreach (Chunk? neighbor in chunk.Neighbors)
                    {
                        if (neighbor?.State is > GenerationState.AwaitingMesh)
                        {
                            _ChunksRequiringRemesh.Current.Enqueue(neighbor);
                        }
                    }

                    // ensure we activate inactive chunks
                    chunk.State += 1;
                }
            }
        }

        private void AssignNeighbors(Vector3<int> origin, Chunk?[] neighbors)
        {
            Chunk? neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(GenerationConstants.CHUNK_SIZE, 0, 0), out neighbor);
            neighbors[0] = neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(0, GenerationConstants.CHUNK_SIZE, 0), out neighbor);
            neighbors[1] = neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(0, 0, GenerationConstants.CHUNK_SIZE), out neighbor);
            neighbors[2] = neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(-GenerationConstants.CHUNK_SIZE, 0, 0), out neighbor);
            neighbors[3] = neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(0, -GenerationConstants.CHUNK_SIZE, 0), out neighbor);
            neighbors[4] = neighbor;

            _VoxelWorld.TryGetChunk(origin + new Vector3<int>(0, 0, -GenerationConstants.CHUNK_SIZE), out neighbor);
            neighbors[5] = neighbor;
        }
    }
}
