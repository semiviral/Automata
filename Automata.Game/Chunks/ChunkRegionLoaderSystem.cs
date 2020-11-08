#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Automata.Engine.Systems;
using Automata.Game.Chunks.Generation;
using Serilog;

#endregion


namespace Automata.Game.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private readonly ChunkMap _ChunkMap;

        public ChunkRegionLoaderSystem() => _ChunkMap = new ChunkMap();

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            List<(Translation Translation, ChunkLoader ChunkLoader)> components = entityManager.GetComponents<Translation, ChunkLoader>().ToList();

            bool recalculateChunkRegions = false;

            foreach ((Translation translation, ChunkLoader chunkLoader) in components)
            {
                Vector3i translationInt32 = Vector3i.FromVector3(translation.Value).SetComponent(1, 0);
                Vector3i difference = Vector3i.Abs(translationInt32 - chunkLoader.Origin);

                if (!chunkLoader.Changed && Vector3b.All(difference < GenerationConstants.CHUNK_SIZE)) continue;

                chunkLoader.Origin = chunkLoader.Origin = Vector3i.RoundBy(translationInt32, GenerationConstants.CHUNK_SIZE);
                recalculateChunkRegions = true;
            }

            if (!recalculateChunkRegions) return;

            HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>(components.SelectMany(loader =>
                GetActiveChunkLoaderRegion(loader.ChunkLoader)));

            IEnumerable<Vector3i> activations = withinLoaderRange.Except(_ChunkMap.Active);
            IEnumerable<Vector3i> deactivations = _ChunkMap.Active.Except(withinLoaderRange);

            int totalActivations = activations.Count(origin => _ChunkMap.TryAdd(entityManager, origin, out IEntity? _));
            int totalDeactivations = deactivations.Count(origin => _ChunkMap.TryRemove(entityManager, origin, out IEntity? _));

            _ChunkMap.RecalculateAllNeighbors();

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                $"Region loading: {totalActivations} activations, {totalDeactivations} deactivations"));
        }

        private static IEnumerable<Vector3i> GetActiveChunkLoaderRegion(ChunkLoader chunkLoader)
        {
            Vector3i chunkLoaderOriginYAdjusted = new Vector3i(chunkLoader.Origin.X, 0, chunkLoader.Origin.Z);

            for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
            for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
            for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                yield return chunkLoaderOriginYAdjusted + (new Vector3i(x, y, z) * GenerationConstants.CHUNK_SIZE);
        }
    }
}
