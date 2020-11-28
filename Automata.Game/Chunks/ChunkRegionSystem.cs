using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly HashSet<Vector3i> _LoadableChunks;

        public ChunkRegionSystem(VoxelWorld voxelWorld) : base(voxelWorld)
        {
            _VoxelWorld = voxelWorld;
            _ChunksRequiringRemesh = new Queue<Chunk>();
            _ChunksPendingDisposal = new Stack<Chunk>();
            _LoadableChunks = new HashSet<Vector3i>();
        }

        public override void Registered(EntityManager entityManager)
        {
            DiagnosticsProvider.EnableGroup<ChunkRegionLoadingDiagnosticGroup>();

            InputManager.Instance.RegisterInputAction(() => Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionSystem),
                $"Average update time: {DiagnosticsProvider.GetGroup<ChunkRegionLoadingDiagnosticGroup>().Average():0.00}ms")), Key.ShiftLeft, Key.X);
            InputManager.Instance.RegisterInputAction(() =>
            {
                foreach (Chunk chunk in _VoxelWorld.Entities.Select(entity => entity.Component<Chunk>().Unwrap()))
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

                    for (; index < chunk!.Neighbors.Length; index++)
                    {
                        if (chunk!.Neighbors[index]?.State is not null and
                            (GenerationState.GeneratingTerrain
                            or GenerationState.GeneratingStructures
                            or GenerationState.GeneratingMesh))
                        {
                            break;
                        }
                    }

                    if (index == chunk!.Neighbors.Length)
                    {
                        chunk.RegionDispose();
                    }
                    else
                    {
                        stackEnumerator.SaveCurrent();
                    }
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
            _LoadableChunks.Clear();

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
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 0, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 1, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 2, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 3, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 4, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 5, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 6, zPos));
                    _LoadableChunks.Add(yAdjustedOrigin + new Vector3i(xPos, GenerationConstants.CHUNK_SIZE * 7, zPos));
                }
            }

            // allocate chunks
            foreach (Vector3i origin in _LoadableChunks)
            {
                await _VoxelWorld.TryAllocate(entityManager, origin);
            }

            // deallocate chunks that aren't within loader radii
            foreach (Vector3i origin in _VoxelWorld.Origins)
            {
                if (!_LoadableChunks.Contains(origin) && _VoxelWorld.TryDeallocate(entityManager, origin, out Chunk? chunk))
                {
                    // todo it would be nice to just dispose of chunks outright, instead of deferring it
                    _ChunksPendingDisposal.Push(chunk);
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
                Chunk chunk = entity.Component<Chunk>().Unwrap();

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
            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(GenerationConstants.CHUNK_SIZE, 0, 0), out Entity? neighbor))
            {
                origins[0] = neighbor.Component<Chunk>().Unwrap();
            }

            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, GenerationConstants.CHUNK_SIZE, 0), out neighbor))
            {
                origins[1] = neighbor.Component<Chunk>().Unwrap();
            }

            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, 0, GenerationConstants.CHUNK_SIZE), out neighbor))
            {
                origins[2] = neighbor.Component<Chunk>().Unwrap();
            }

            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(-GenerationConstants.CHUNK_SIZE, 0, 0), out neighbor))
            {
                origins[3] = neighbor.Component<Chunk>().Unwrap();
            }

            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, -GenerationConstants.CHUNK_SIZE, 0), out neighbor))
            {
                origins[4] = neighbor.Component<Chunk>().Unwrap();
            }

            if (_VoxelWorld.TryGetChunkEntity(origin + new Vector3i(0, 0, -GenerationConstants.CHUNK_SIZE), out neighbor))
            {
                origins[5] = neighbor.Component<Chunk>().Unwrap();
            }
        }

        #endregion
    }
}
