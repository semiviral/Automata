#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private VoxelWorld VoxelWorld => _CurrentWorld as VoxelWorld ?? throw new InvalidOperationException("Must be in VoxelWorld.");

        [HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(ChunkLoader))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            bool recalculateChunkRegions = false;
            IEnumerable<(Translation Translation, ChunkLoader ChunkLoader)> components = entityManager.GetComponents<Translation, ChunkLoader>();

            foreach ((Translation translation, ChunkLoader chunkLoader) in components)
            {
                Vector3i translationInt32 = Vector3i.FromVector3(translation.Value).SetComponent(1, 0);
                Vector3i difference = Vector3i.Abs(translationInt32 - chunkLoader.Origin);

                if (!chunkLoader.Changed && Vector3b.All(difference < GenerationConstants.CHUNK_SIZE)) continue;

                chunkLoader.Origin = chunkLoader.Origin = Vector3i.RoundBy(translationInt32, GenerationConstants.CHUNK_SIZE);
                recalculateChunkRegions = true;
            }

            if (recalculateChunkRegions)
            {
                HashSet<Vector3i> withinLoaderRange = new HashSet<Vector3i>(components.SelectMany(loader =>
                    GetActiveChunkLoaderRegion(loader.ChunkLoader)));

                IEnumerable<Vector3i> activations = withinLoaderRange.Except(VoxelWorld.Chunks.Origins);
                IEnumerable<Vector3i> deactivations = VoxelWorld.Chunks.Origins.Except(withinLoaderRange);

                int totalActivations = activations.Count(origin => VoxelWorld.Chunks.TryAdd(entityManager, origin, out IEntity? _));
                int totalDeactivations = deactivations.Count(origin => VoxelWorld.Chunks.TryRemove(entityManager, origin, out IEntity? _));

                VoxelWorld.Chunks.RecalculateAllChunkNeighbors();

                Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                    $"Region loading: {totalActivations} activations, {totalDeactivations} deactivations"));
            }

            return ValueTask.CompletedTask;
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
