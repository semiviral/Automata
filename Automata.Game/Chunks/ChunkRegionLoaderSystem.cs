#region

using System;
using System.Collections.Generic;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Systems;
using Automata.Game.Chunks.Generation;
using Serilog;

#endregion

namespace Automata.Game.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private readonly Dictionary<Vector3i, IEntity> _ChunkEntities;
        private readonly HashSet<Vector3i> _ActivatingChunks;
        private readonly HashSet<Vector3i> _NotWithinLoaderRange;
        private readonly HashSet<Vector3i> _WithinLoaderRange;
        private readonly Stack<Vector3i> _ActivationPendingChunks;
        private readonly Stack<Vector3i> _DeactivationPendingChunks;

        public ChunkRegionLoaderSystem()
        {
            _ChunkEntities = new Dictionary<Vector3i, IEntity>();
            _ActivatingChunks = new HashSet<Vector3i>();
            _NotWithinLoaderRange = new HashSet<Vector3i>();
            _WithinLoaderRange = new HashSet<Vector3i>();
            _ActivationPendingChunks = new Stack<Vector3i>();
            _DeactivationPendingChunks = new Stack<Vector3i>();
        }

        private static bool IsWithinLoaderRange(Vector3i difference, ChunkLoader chunkLoader) =>
            Vector3b.All(difference <= (GenerationConstants.CHUNK_SIZE * chunkLoader.Radius));

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!TryRecalculateRegions(entityManager))
            {
                return;
            }

            AllocateChunkChanges();
            HandleAllocatedChunkChanges(entityManager);
        }

        private bool TryRecalculateRegions(EntityManager entityManager)
        {
            bool hasRecalculatedRegion = false;

            foreach ((Translation translation, ChunkLoader chunkLoader) in entityManager.GetComponents<Translation, ChunkLoader>())
            {
                Vector3i difference = Vector3i.Abs(Vector3i.FromVector3(translation.Value) - chunkLoader.Origin);
                if (Vector3b.All(difference < GenerationConstants.CHUNK_SIZE))
                {
                    continue;
                }

                hasRecalculatedRegion = true;
                Vector3i chunkLoaderOldOrigin = chunkLoader.Origin;
                chunkLoader.Origin = Vector3i.RoundBy(Vector3i.FromVector3(translation.Value), GenerationConstants.CHUNK_SIZE);

                Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                    $"Recalculating chunk region: old {chunkLoaderOldOrigin}, new {chunkLoader.Origin}"));

                // allocate list of chunks requiring activation
                for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                {
                    Vector3i localOrigin = new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE;
                    Vector3i globalOrigin = localOrigin + new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

                    // do not add chunks that already exist
                    if (_ChunkEntities.ContainsKey(globalOrigin))
                    {
                        continue;
                    }

                    _ActivatingChunks.Add(globalOrigin);
                }

                // allocate list of chunks requiring deactivation
                foreach ((Vector3i origin, IEntity _) in _ChunkEntities)
                {
                    Vector3i chunkDifference = Vector3i.Abs(origin - chunkLoader.Origin);

                    if (IsWithinLoaderRange(chunkDifference, chunkLoader))
                    {
                        _WithinLoaderRange.Add(origin);
                    }
                    else
                    {
                        _NotWithinLoaderRange.Add(origin);
                    }
                }
            }

            return hasRecalculatedRegion;
        }

        private void AllocateChunkChanges()
        {
            // except with origins that ARE within loader range
            _NotWithinLoaderRange.ExceptWith(_WithinLoaderRange);

            foreach (Vector3i origin in _ActivatingChunks)
            {
                _ActivationPendingChunks.Push(origin);
            }

            foreach (Vector3i origin in _NotWithinLoaderRange)
            {
                _DeactivationPendingChunks.Push(origin);
            }

            _ActivatingChunks.Clear();
            _NotWithinLoaderRange.Clear();
            _WithinLoaderRange.Clear();
        }

        private void HandleAllocatedChunkChanges(EntityManager entityManager)
        {
            int activations = _ActivationPendingChunks.Count;
            int deactivations = _DeactivationPendingChunks.Count;

            if ((_ActivationPendingChunks.Count <= 0) && (_DeactivationPendingChunks.Count <= 0))
            {
                return;
            }

            while (_ActivationPendingChunks.TryPop(out Vector3i origin))
            {
                IEntity chunk = entityManager.ComposeEntity<ChunkComposition>(true);
                chunk.GetComponent<Translation>().Value = origin;
                chunk.GetComponent<Chunk>().State = GenerationState.Ungenerated;

                _ChunkEntities.Add(origin, chunk);
            }

            while (_DeactivationPendingChunks.TryPop(out Vector3i origin))
            {
                _ChunkEntities.Remove(origin, out IEntity? entity);

                if (entity is null)
                {
                    continue;
                }

                entityManager.RemoveEntity(entity);
            }

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                $"Region loading: {activations} activations, {deactivations} deactivations"));
        }
    }
}
