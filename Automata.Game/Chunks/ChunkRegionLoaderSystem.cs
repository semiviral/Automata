#region

using System;
using System.Collections.Generic;
using System.Linq;
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

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            List<(Translation Translation, ChunkLoader ChunkLoader)> components = entityManager.GetComponents<Translation, ChunkLoader>().ToList();

            bool recalculateChunkRegions = false;
            foreach ((Translation translation, ChunkLoader chunkLoader) in components)
            {
                Vector3i translationVector3i = Vector3i.FromVector3(translation.Value);
                Vector3i difference = Vector3i.Abs(translationVector3i - chunkLoader.Origin);

                if (Vector3b.Any(difference >= GenerationConstants.CHUNK_SIZE))
                {
                    chunkLoader.Origin = chunkLoader.Origin = Vector3i.RoundBy(translationVector3i, GenerationConstants.CHUNK_SIZE);
                    recalculateChunkRegions = true;
                }
            }

            if (!recalculateChunkRegions)
            {
                return;
            }

            HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>(components.SelectMany(loader =>
                GetActiveChunkLoaderRegion(loader.ChunkLoader)));

            IEnumerable<Vector3i> activations = withinLoaderRange.Except(_ChunkEntities.Keys);
            IEnumerable<Vector3i> deactivations = _ChunkEntities.Keys.Except(withinLoaderRange);

            int totalActivations = 0, totalDeactivations = 0;

            foreach (Vector3i origin in activations)
            {
                IEntity chunk = entityManager.ComposeEntity<ChunkComposition>(true);
                chunk.GetComponent<Translation>().Value = origin;
                chunk.GetComponent<Chunk>().State = GenerationState.Ungenerated;

                _ChunkEntities.Add(origin, chunk);

                totalActivations += 1;
            }

            foreach (Vector3i origin in deactivations)
            {
                _ChunkEntities.Remove(origin, out IEntity? entity);

                if (entity is null)
                {
                    continue;
                }

                entityManager.RemoveEntity(entity);

                totalDeactivations += 1;
            }

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                $"Region loading: {totalActivations} activations, {totalDeactivations} deactivations"));
        }

        private static IEnumerable<Vector3i> GetActiveChunkLoaderRegion(ChunkLoader chunkLoader)
        {
            Vector3i chunkLoaderOriginYAdjusted = new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);
            for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
            for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
            for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
            {
                Vector3i localOrigin = new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE;
                yield return localOrigin + chunkLoaderOriginYAdjusted;
            }
        }
    }
}
