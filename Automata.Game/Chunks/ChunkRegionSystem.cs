using System;
using System.Buffers;
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
        }

        [HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(ChunkLoader))]
        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            using (SavableQueueEnumerator<Chunk> enumerator = new SavableQueueEnumerator<Chunk>(_ChunksRequiringRemesh))
            {
                while (enumerator.MoveNext())
                {
                    Chunk chunk = enumerator.Current;

                    switch (chunk!.State)
                    {
                        case GenerationState.GeneratingMesh:
                            enumerator.SaveCurrent();
                            break;
                        case GenerationState.Finished:
                            chunk.State = GenerationState.AwaitingMesh;
                            break;
                    }
                }
            }

            // process chunks requires disposal
            while (_ChunksPendingDisposal.TryPeek(out Chunk? chunk)
                   && Array.TrueForAll(chunk!.Neighbors, neighbor =>
                       neighbor?.State is null
                           or not GenerationState.GeneratingTerrain
                           and not GenerationState.GeneratingStructures
                           and not GenerationState.GeneratingMesh)
                   && _ChunksPendingDisposal.TryPop(out chunk))
            {
                chunk!.RegionDispose();
            }

            // determine whether any chunk loaders have moved out far enough to recalculate their loaded chunk region
            if (CheckAndUpdateChunkLoaders(entityManager))
            {
                await RecalculateRegion(entityManager);
            }
        }

        private static bool CheckAndUpdateChunkLoaders(EntityManager entityManager)
        {
            bool updatedChunkPositions = false;

            foreach ((Transform transform, ChunkLoader chunkLoader) in entityManager.GetComponents<Transform, ChunkLoader>())
            {
                Vector3 difference = Vector3.Abs(transform.Translation - chunkLoader.Origin);

                if (!chunkLoader.Changed && (difference.X < GenerationConstants.CHUNK_SIZE) && (difference.Z < GenerationConstants.CHUNK_SIZE))
                {
                    continue;
                }

                chunkLoader.Origin = Vector3i.FromVector3(transform.Translation.RoundBy(GenerationConstants.CHUNK_SIZE));
                chunkLoader.Changed = false;
                updatedChunkPositions = true;
            }

            return updatedChunkPositions;
        }


        #region RecalculateRegion

        private async ValueTask RecalculateRegion(EntityManager entityManager)
        {
            HashSet<Vector3i> withinLoaderRange = GetOriginsWithLoaderRanges(entityManager);

            foreach (Vector3i origin in withinLoaderRange)
            {
                await _VoxelWorld.TryAllocate(entityManager, origin);
            }

            foreach (Vector3i origin in _VoxelWorld.Origins.Except(withinLoaderRange))
            {
                if (!withinLoaderRange.Contains(origin) && _VoxelWorld.TryDeallocate(entityManager, origin, out Chunk? chunk))
                {
                    _ChunksPendingDisposal.Push(chunk);
                }
            }

            UpdateRegionState();
        }

        private static HashSet<Vector3i> GetOriginsWithLoaderRanges(EntityManager entityManager)
        {
            HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>();

            foreach (ChunkLoader chunkLoader in entityManager.GetComponents<ChunkLoader>())
            {
                Vector3i chunkLoaderOrigin = new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

                for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                {
                    withinLoaderRange.Add(chunkLoaderOrigin + (new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE));
                }
            }

            return withinLoaderRange;
        }

        #endregion


        #region UpdateRegionState

        private void UpdateRegionState()
        {
            Chunk?[] neighbors = ArrayPool<Chunk?>.Shared.Rent(6);

            foreach ((Vector3i origin, Entity entity) in _VoxelWorld)
            {
                if (entity.TryComponent(out Chunk? chunk))
                {
                    // here we assign this chunk's neighbors
                    //
                    // in addition, if this chunk is inactive (i.e. a new allocation) then
                    // we also enqueue each neighbor the a queue, signifying that once the neighbor
                    // enter the 'GenerationState.Finished' state, it needs to be remeshed.
                    GetNeighborsOfOrigin(origin, neighbors);

                    for (int neighborIndex = 0; neighborIndex < 6; neighborIndex++)
                    {
                        Chunk? neighbor = neighbors[neighborIndex];

                        if (chunk.State is GenerationState.Inactive && (neighbor?.State > GenerationState.AwaitingMesh))
                        {
                            _ChunksRequiringRemesh.Enqueue(neighbor);
                        }

                        chunk.Neighbors[neighborIndex] = neighbor;
                    }

                    // ensure we activate inactive chunks
                    if (chunk.State is GenerationState.Inactive)
                    {
                        chunk.State += 1;
                    }
                }
            }

            ArrayPool<Chunk?>.Shared.Return(neighbors);
        }

        private void GetNeighborsOfOrigin(Vector3i origin, Chunk?[] origins)
        {
            for (int normalIndex = 0; normalIndex < 6; normalIndex++)
            {
                int sign = (normalIndex - 3) >= 0 ? -1 : 1;
                int componentIndex = normalIndex % 3;
                Vector3i component = Vector3i.One.WithComponent<Vector3i, int>(componentIndex) * sign;
                Vector3i neighborOrigin = origin + (component * GenerationConstants.CHUNK_SIZE);

                _VoxelWorld.TryGetChunkEntity(neighborOrigin, out Entity? neighbor);
                origins[normalIndex] = neighbor?.Component<Chunk>();
            }
        }

        #endregion
    }
}
