using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Extensions;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Game.Chunks.Generation;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input;

namespace Automata.Game.Chunks
{
    public class ChunkRegionSystem : ComponentSystem
    {
        private readonly VoxelWorld _VoxelWorld;
        private readonly Stack<Chunk> _ChunksPendingDisposal;
        private readonly Queue<Chunk> _ChunksRequiringRemesh;

        public ChunkRegionSystem(VoxelWorld voxelWorld) : base(voxelWorld)
        {
            _VoxelWorld = voxelWorld;
            _ChunksRequiringRemesh = new Queue<Chunk>();
            _ChunksPendingDisposal = new Stack<Chunk>();
        }

        public override void Registered(EntityManager entityManager)
        {
            DiagnosticsProvider.EnableGroup<ChunkRegionLoadingDiagnosticGroup>();

            InputManager.Instance.RegisterInputAction(() => Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionSystem),
                $"Average update time: {DiagnosticsProvider.GetGroup<ChunkRegionLoadingDiagnosticGroup>().Average():0.00}ms")), Key.ShiftLeft, Key.X);

            InputManager.Instance.RegisterInputAction(() =>
            {
                foreach (Chunk chunk in _VoxelWorld.Entities.Select(entity => entity.Component<Chunk>()!))
                {
                    chunk!.State = GenerationState.AwaitingMesh;
                }
            }, Key.ShiftLeft, Key.R);
        }

        [HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(ChunkLoader))]
        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            using (SavableQueueEnumerator<Chunk> queueEnumerator = new SavableQueueEnumerator<Chunk>(_ChunksRequiringRemesh))
            {
                while (queueEnumerator.MoveNext())
                {
                    Chunk chunk = queueEnumerator.Current;

                    switch (chunk!.State)
                    {
                        case GenerationState.GeneratingMesh:
                            queueEnumerator.SaveCurrent();
                            break;
                        case GenerationState.Finished:
                            chunk.State = GenerationState.AwaitingMesh;
                            break;
                    }
                }
            }

            using (SavableStackEnumerator<Chunk> stackEnumerator = new SavableStackEnumerator<Chunk>(_ChunksPendingDisposal))
            {
                while (stackEnumerator.MoveNext())
                {
                    Chunk chunk = stackEnumerator.Current;

                    int index = 0;

                    if (!chunk.IsGenerating)
                    {
                        for (; index < chunk!.Neighbors.Length; index++)
                        {
                            if (chunk!.Neighbors[index]?.IsGenerating is true)
                            {
                                break;
                            }
                        }

                        if (index == chunk!.Neighbors.Length)
                        {
                            chunk.RegionDispose();
                            continue;
                        }
                    }

                    stackEnumerator.SaveCurrent();
                }
            }

            // determine whether any chunk loaders have moved out far enough to recalculate their loaded chunk region
            if (UpdateChunkLoaders(entityManager))
            {
                await RecalculateLoadedRegions(entityManager);
            }
        }

        private static bool UpdateChunkLoaders(EntityManager entityManager)
        {
            bool updatedChunkPositions = false;

            foreach ((Transform transform, ChunkLoader chunkLoader) in entityManager.GetComponents<Transform, ChunkLoader>())
            {
                Vector3 difference = Vector3.Abs(transform.Translation - chunkLoader.Origin);

                if (!chunkLoader.RadiusChanged && (difference.X < GenerationConstants.CHUNK_SIZE) && (difference.Z < GenerationConstants.CHUNK_SIZE))
                {
                    continue;
                }

                chunkLoader.Origin = Vector3i.FromVector3(transform.Translation.RoundBy(GenerationConstants.CHUNK_SIZE));
                chunkLoader.RadiusChanged = false;
                updatedChunkPositions = true;
            }

            return updatedChunkPositions;
        }


        #region RecalculateRegion

        private async ValueTask RecalculateLoadedRegions(EntityManager entityManager)
        {
            // calculate all loadable chunk origins
            foreach (ChunkLoader chunkLoader in entityManager.GetComponents<ChunkLoader>())
            {
                Vector3i yAdjustedOrigin = new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                {
                    int xPos = x * GenerationConstants.CHUNK_SIZE;
                    int zPos = z * GenerationConstants.CHUNK_SIZE;

                    // remark: this relies on GenerationConstants.WORLD_HEIGHT_IN_CHUNKS being 8
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 0, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 1, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 2, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 3, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 4, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 5, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 6, zPos));
                    await _VoxelWorld.TryAllocate(entityManager, yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 7, zPos));
                }
            }

            // deallocate chunks that aren't within loader radii
            foreach ((Vector3i origin, Entity entity) in _VoxelWorld)
            {
                bool disposable = true;
                foreach (ChunkLoader loader in entityManager.GetComponents<ChunkLoader>())
                {
                    if (loader.IsWithinRadius(origin))
                    {
                        disposable = false;
                    }
                }

                if (disposable)
                {
                    // todo it would be nice to just dispose of chunks outright, instead of deferring it
                    _VoxelWorld.TryDeallocate(origin);
                    _ChunksPendingDisposal.Push(entity.Component<Chunk>()!);
                    entityManager.RemoveEntity(entity);
                }
            }

            UpdateRegionState();
        }

        #endregion


        #region UpdateRegionState

        private void UpdateRegionState()
        {
            foreach ((Vector3i origin, Entity entity) in _VoxelWorld)
            {
                Chunk chunk = entity.Component<Chunk>()!;

                // here we assign this chunk's neighbors
                //
                // in addition, if this chunk is inactive (i.e. a new allocation) then
                // we also enqueue each neighbor the a queue, signifying that once the neighbor
                // enter the 'GenerationState.Finished' state, it needs to be remeshed.
                AssignNeighbors(origin, chunk.Neighbors);

                if (chunk.State is GenerationState.Inactive)
                {
                    foreach (Chunk? neighbor in chunk.Neighbors)
                    {
                        if (neighbor?.State is > GenerationState.AwaitingMesh)
                        {
                            _ChunksRequiringRemesh.Enqueue(neighbor);
                        }
                    }

                    // ensure we activate inactive chunks
                    chunk.State += 1;
                }
            }
        }

        private void AssignNeighbors(Vector3i origin, Chunk?[] origins)
        {
            Entity? neighbor;

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(GenerationConstants.CHUNK_SIZE, 0, 0), out neighbor);
            origins[0] = neighbor?.Component<Chunk>();

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, GenerationConstants.CHUNK_SIZE, 0), out neighbor);
            origins[1] = neighbor?.Component<Chunk>();

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, 0, GenerationConstants.CHUNK_SIZE), out neighbor);
            origins[2] = neighbor?.Component<Chunk>();

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(-GenerationConstants.CHUNK_SIZE, 0, 0), out neighbor);
            origins[3] = neighbor?.Component<Chunk>();

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, -GenerationConstants.CHUNK_SIZE, 0), out neighbor);
            origins[4] = neighbor?.Component<Chunk>();

            _VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, 0, -GenerationConstants.CHUNK_SIZE), out neighbor);
            origins[5] = neighbor?.Component<Chunk>();
        }

        #endregion
    }
}
