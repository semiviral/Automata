using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Diagnostics;
using Automata.Engine.Extensions;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Game.Chunks.Generation;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input.Common;

namespace Automata.Game.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private readonly Stack<Chunk> _ChunksPendingCleanup;
        private readonly Queue<Chunk> _ChunksRequiringRemesh;

        private VoxelWorld VoxelWorld => _CurrentWorld as VoxelWorld ?? throw new InvalidOperationException("Must be in VoxelWorld.");

        public ChunkRegionLoaderSystem()
        {
            _ChunksRequiringRemesh = new Queue<Chunk>();
            _ChunksPendingCleanup = new Stack<Chunk>();
        }

        public override void Registered(EntityManager entityManager)
        {
            DiagnosticsProvider.EnableGroup<ChunkRegionLoadingDiagnosticGroup>();

            InputManager.Instance.RegisterInputAction(() => Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                $"Average update time: {DiagnosticsProvider.GetGroup<ChunkRegionLoadingDiagnosticGroup>().Average():0.00}ms")), Key.ShiftLeft, Key.X);
        }

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            while (_ChunksRequiringRemesh.TryPeek(out Chunk? chunk)
                   && chunk!.State is not GenerationState.GeneratingMesh
                   && _ChunksRequiringRemesh.TryDequeue(out chunk))
            {
                if (chunk!.State is GenerationState.Finished)
                {
                    chunk.State = GenerationState.AwaitingMesh;
                }
            }

            while (_ChunksPendingCleanup.TryPeek(out Chunk? chunk)
                   && Array.TrueForAll(chunk!.Neighbors, neighbor => neighbor?.State is null
                       or not GenerationState.GeneratingTerrain
                       and not GenerationState.GeneratingStructures
                       and not GenerationState.GeneratingMesh)
                   && _ChunksPendingCleanup.TryPop(out chunk))
            {
                chunk!.SafeDispose();
            }

            // determine whether any chunk loaders have moved out far enough to recalculate their loaded chunk region
            if (CheckAndUpdateChunkLoaderPositions(entityManager))
            {
                RecalculateChunkRegions(entityManager);
                await VoxelWorld.Chunks.ProcessPendingChunkAllocations();
            }

            DiagnosticsProvider.CommitData<ChunkRegionLoadingDiagnosticGroup, TimeSpan>(new RegionLoadingTime(stopwatch.Elapsed));
            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private static bool CheckAndUpdateChunkLoaderPositions(EntityManager entityManager)
        {
            bool updatedChunkPositions = false;

            foreach ((Translation translation, ChunkLoader chunkLoader) in entityManager.GetComponents<Translation, ChunkLoader>())
            {
                // remove y-component of translation
                Vector3i translationInt32 = Vector3i.FromVector3(translation.Value).SetComponent(1, 0);
                Vector3i difference = Vector3i.Abs(translationInt32 - chunkLoader.Origin);

                if (!chunkLoader.Changed && Vector3b.All(difference < GenerationConstants.CHUNK_SIZE))
                {
                    continue;
                }

                chunkLoader.Origin = Vector3i.RoundBy(translationInt32, GenerationConstants.CHUNK_SIZE);
                updatedChunkPositions = true;
            }

            return updatedChunkPositions;
        }

        private void RecalculateChunkRegions(EntityManager entityManager)
        {
            // this calculates new chunk allocations and current chunk deallocations
            HashSet<Vector3i> withinLoaderRange = new(GetOriginsWithinLoaderRanges(entityManager.GetComponents<ChunkLoader>()));

            foreach (Vector3i origin in withinLoaderRange.Except(VoxelWorld.Chunks.Origins))
            {
                VoxelWorld.Chunks.Allocate(entityManager, origin);
            }

            foreach (Vector3i origin in VoxelWorld.Chunks.Origins.Except(withinLoaderRange))
            {
                if (VoxelWorld.Chunks.TryDeallocate(entityManager, origin, out Chunk? chunk))
                {
                    _ChunksPendingCleanup.Push(chunk);
                }
            }

            // here we update neighbors, and allocate (in a stack) all chunks that will require remeshing
            foreach ((Vector3i origin, IEntity entity) in VoxelWorld.Chunks)
            {
                if (!entity.TryFind(out Chunk? chunk))
                {
                    continue;
                }

                // here we assign this chunk's neighbors
                //
                // in addition, if this chunk is inactive (i.e. a new allocation) then
                // we also enqueue each neighbor the a queue, signifying that once the neighbor
                // enter the 'GenerationState.Finished' state, it needs to be remeshed.
                int neighborIndex = 0;
                bool isNewChunk = chunk.State is GenerationState.Inactive;

                if (isNewChunk)
                {
                    chunk.State += 1; // chunk is being processed, so is not inactive
                }

                foreach (Chunk? neighbor in GetNeighborsOfOrigin(origin))
                {
                    if (isNewChunk && neighbor?.State is > GenerationState.AwaitingMesh)
                    {
                        _ChunksRequiringRemesh.Enqueue(neighbor);
                    }

                    chunk.Neighbors[neighborIndex] = neighbor;
                    neighborIndex += 1;
                }
            }
        }

        private static IEnumerable<Vector3i> GetOriginsWithinLoaderRanges(IEnumerable<ChunkLoader> enumerable)
        {
            foreach (ChunkLoader chunkLoader in enumerable)
            {
                Vector3i chunkLoaderOriginYAdjusted = new(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

                for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                {
                    yield return chunkLoaderOriginYAdjusted + (new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE);
                }
            }
        }

        private IEnumerable<Chunk?> GetNeighborsOfOrigin(Vector3i origin)
        {
            for (int normalIndex = 0; normalIndex < 6; normalIndex++)
            {
                int sign = (normalIndex - 3) >= 0 ? -1 : 1;
                int componentIndex = normalIndex % 3;
                Vector3i component = Vector3i.One.WithComponent<Vector3i, int>(componentIndex) * sign;
                Vector3i neighborOrigin = origin + (component * GenerationConstants.CHUNK_SIZE);

                VoxelWorld.Chunks.TryGetChunkEntity(neighborOrigin, out IEntity? neighbor);
                yield return neighbor?.Find<Chunk>();
            }
        }
    }
}
