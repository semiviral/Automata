#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Automata.Engine.Systems;
using Automata.Game.Chunks.Generation;

#endregion


namespace Automata.Game.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private readonly Queue<Chunk> _PendingRemeshingDueToNeighborUpdatesQueue;

        private VoxelWorld VoxelWorld => _CurrentWorld as VoxelWorld ?? throw new InvalidOperationException("Must be in VoxelWorld.");

        public ChunkRegionLoaderSystem() => _PendingRemeshingDueToNeighborUpdatesQueue = new Queue<Chunk>();

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            // attempt to state change any neighbors requiring remesh
            while (_PendingRemeshingDueToNeighborUpdatesQueue.TryPeek(out Chunk? chunk) && chunk.State is GenerationState.Finished)
            {
                _PendingRemeshingDueToNeighborUpdatesQueue.Dequeue();
                chunk.State = GenerationState.AwaitingMesh;
            }

            // determine whether any chunk loaders have moved out far enough to recalculate their loaded chunk region
            bool recalculateChunkRegions = false;

            foreach ((Translation translation, ChunkLoader chunkLoader) in entityManager.GetComponents<Translation, ChunkLoader>())
            {
                // remove y-component of translation
                Vector3i translationInt32 = Vector3i.FromVector3(translation.Value).SetComponent(1, 0);
                Vector3i difference = Vector3i.Abs(translationInt32 - chunkLoader.Origin);

                if (!chunkLoader.Changed && Vector3b.All(difference < GenerationConstants.CHUNK_SIZE)) continue;

                chunkLoader.Origin = Vector3i.RoundBy(translationInt32, GenerationConstants.CHUNK_SIZE);
                recalculateChunkRegions = true;
            }

            if (recalculateChunkRegions)
            {
                // this calculates new chunk allocations and current chunk deallocations
                HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>(GetOriginsWithinLoaderRanges(entityManager.GetComponents<ChunkLoader>()));

                // process chunk allocations
                foreach (Vector3i origin in withinLoaderRange.Except(VoxelWorld.Chunks.Origins))
                    if (VoxelWorld.Chunks.TryAllocate(origin, out IEntity? entity))
                        entityManager.RegisterEntity(entity);

                // process chunk deallocations
                foreach (Vector3i origin in VoxelWorld.Chunks.Origins.Except(withinLoaderRange))
                    if (VoxelWorld.Chunks.TryDeallocate(origin, out IEntity? entity))
                        entityManager.RemoveEntity(entity);

                // here we update neighbors, and allocate (in a stack) all chunks that will require remeshing
                foreach ((Vector3i origin, IEntity entity) in VoxelWorld.Chunks)
                {
                    Chunk? chunk = entity.Find<Chunk>();

                    Debug.Assert(chunk is not null, "All entities within VoxelWorld.Chunks should have Chunk component.");

                    // here we assign this chunk's neighbors
                    //
                    // in addition, if this chunk is inactive (i.e. a new allocation) then
                    // we also enqueue each neighbor the a queue, signifying that once the neighbor
                    // enter the 'GenerationState.Finished' state, it needs to be remeshed.
                    int neighborIndex = 0;
                    bool isNewChunk = chunk.State is GenerationState.Inactive;
                    if (isNewChunk) chunk.State += 1; // chunk is being processed, so is not inactive

                    foreach (Chunk? neighbor in GetNeighborsOfOrigin(origin))
                    {
                        if (isNewChunk && neighbor is not null) _PendingRemeshingDueToNeighborUpdatesQueue.Enqueue(neighbor);

                        chunk.Neighbors[neighborIndex] = neighbor;
                        neighborIndex += 1;
                    }
                }
            }

            return ValueTask.CompletedTask;
        }

        private IEnumerable<Chunk?> GetNeighborsOfOrigin(Vector3i origin)
        {
            for (int normalIndex = 0; normalIndex < 6; normalIndex++)
            {
                int sign = (normalIndex - 3) >= 0 ? -1 : 1;
                int componentIndex = normalIndex % 3;
                Vector3i component = Vector3i.One.WithComponent<Vector3i, int>(componentIndex) * sign;
                Vector3i neighborOrigin = origin + (component * GenerationConstants.CHUNK_SIZE);

                VoxelWorld.Chunks.TryGetEntity(neighborOrigin, out IEntity? neighbor);
                yield return neighbor?.Find<Chunk>();
            }
        }

        private static IEnumerable<Vector3i> GetOriginsWithinLoaderRanges(IEnumerable<ChunkLoader> enumerable)
        {
            static IEnumerable<Vector3i> GetActiveChunkLoaderRegionImpl(ChunkLoader chunkLoader)
            {
                Vector3i chunkLoaderOriginYAdjusted = new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

                for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                    yield return chunkLoaderOriginYAdjusted + (new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE);
            }

            foreach (ChunkLoader chunkLoader in enumerable)
            foreach (Vector3i origin in GetActiveChunkLoaderRegionImpl(chunkLoader))
                yield return origin;
        }
    }
}
