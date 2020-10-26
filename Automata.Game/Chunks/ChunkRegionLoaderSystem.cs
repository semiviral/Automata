#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Systems;
using Automata.Game.Chunks.Generation;
using Serilog;

#endregion


namespace Automata.Game.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private readonly Dictionary<Vector3i, IEntity> _ChunkEntities;

        public ChunkRegionLoaderSystem() => _ChunkEntities = new Dictionary<Vector3i, IEntity>();

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            List<(Translation Translation, ChunkLoader ChunkLoader)> components = entityManager.GetComponents<Translation, ChunkLoader>().ToList();

            bool recalculateChunkRegions = false;

            foreach ((Translation translation, ChunkLoader chunkLoader) in components)
            {
                Vector3i translationInt32 = Vector3i.FromVector3(translation.Value);
                Vector3i difference = Vector3i.Abs(translationInt32 - chunkLoader.Origin);

                if (Vector3b.All(difference < GenerationConstants.CHUNK_SIZE)) continue;

                chunkLoader.Origin = chunkLoader.Origin = Vector3i.RoundBy(translationInt32, GenerationConstants.CHUNK_SIZE);
                recalculateChunkRegions = true;
            }

            if (!recalculateChunkRegions) return;

            HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>(components.SelectMany(loader =>
                GetActiveChunkLoaderRegion(loader.ChunkLoader)));

            IEnumerable<Vector3i> activations = withinLoaderRange.Except(_ChunkEntities.Keys);
            IEnumerable<Vector3i> deactivations = _ChunkEntities.Keys.Except(withinLoaderRange);

            int totalActivations = 0, totalDeactivations = 0;

            foreach (Vector3i origin in deactivations)
            {
                _ChunkEntities.Remove(origin, out IEntity? entity);

                if (entity is null) continue;

                entityManager.RemoveEntity(entity);

                totalDeactivations += 1;
            }

            foreach (Vector3i origin in activations)
            {
                IEntity chunk = entityManager.ComposeEntity<ChunkComposition>(true);
                chunk.GetComponent<Translation>().Value = origin;
                chunk.GetComponent<Chunk>().State = GenerationState.Ungenerated;

                Bounds bounds = chunk.GetComponent<Bounds>();
                bounds.Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS);
                bounds.Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE));
                _ChunkEntities.Add(origin, chunk);

                totalActivations += 1;
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
