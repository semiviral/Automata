#region

using System;
using System.Collections.Generic;
using System.Numerics;
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
        private readonly Dictionary<Vector3, IEntity> _ChunkEntities;

        private readonly HashSet<Vector3> _ActivatingChunks;
        private readonly HashSet<Vector3> _DeactivatingChunks;

        private readonly Stack<Vector3> _ActivationPendingChunks;
        private readonly Stack<Vector3> _DeactivationPendingChunks;

        public ChunkRegionLoaderSystem()
        {
            _ChunkEntities = new Dictionary<Vector3, IEntity>();
            _ActivatingChunks = new HashSet<Vector3>();
            _DeactivatingChunks = new HashSet<Vector3>();
            _ActivationPendingChunks = new Stack<Vector3>();
            _DeactivationPendingChunks = new Stack<Vector3>();

            HandledComponents = new ComponentTypes(typeof(Translation), typeof(ChunkLoader));
        }

        private static bool IsWithinLoaderRange(Vector3i difference, ChunkLoader chunkLoader) =>
            Vector3b.All(difference <= (GenerationConstants.CHUNK_SIZE * chunkLoader.Radius));

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach ((Translation translation, ChunkLoader chunkLoader) in entityManager.GetComponents<Translation, ChunkLoader>())
            {
                if (!translation.Changed)
                {
                    continue;
                }

                Vector3 roundedTranslation = translation.Value.RoundBy(new Vector3(GenerationConstants.CHUNK_SIZE));

                // allocate list of chunks requiring deactivation
                foreach ((Vector3 origin, IEntity _) in _ChunkEntities)
                {
                    Vector3i difference = Vector3i.Abs(Vector3i.FromVector3(origin - roundedTranslation));

                    if (IsWithinLoaderRange(difference, chunkLoader))
                    {
                        _DeactivatingChunks.Remove(origin);
                    }
                    else
                    {
                        _DeactivatingChunks.Add(origin);
                    }
                }

                for (int y = 0; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                for (int z = -chunkLoader.Radius; z < (chunkLoader.Radius + 1); z++)
                for (int x = -chunkLoader.Radius; x < (chunkLoader.Radius + 1); x++)
                {
                    Vector3 localOrigin = new Vector3(x, y, z) * GenerationConstants.CHUNK_SIZE;
                    Vector3 globalOrigin = localOrigin + new Vector3(roundedTranslation.X, 0f, roundedTranslation.Z);

                    _ActivatingChunks.Add(globalOrigin);
                }

                _ActivatingChunks.RemoveWhere(origin => _ChunkEntities.ContainsKey(origin));
                foreach (Vector3 origin in _ActivatingChunks)
                {
                    _ActivationPendingChunks.Push(origin);
                }

                foreach (Vector3 origin in _DeactivatingChunks)
                {
                    _DeactivationPendingChunks.Push(origin);
                }

                _ActivatingChunks.Clear();
                _DeactivatingChunks.Clear();
            }

            HandlePendingChunks(entityManager);
        }

        private void HandlePendingChunks(EntityManager entityManager)
        {
            if ((_ActivationPendingChunks.Count == 0) && (_DeactivationPendingChunks.Count == 0))
            {
                return;
            }

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkRegionLoaderSystem),
                $"Region loading: {_ActivationPendingChunks.Count} activations, {_DeactivationPendingChunks.Count} deactivations"));

            while (_ActivationPendingChunks.TryPop(out Vector3 origin))
            {
                IEntity chunk = entityManager.ComposeEntity<ChunkComposition>(true);
                chunk.GetComponent<Translation>().Value = origin;
                chunk.GetComponent<ChunkState>().Value = GenerationState.Ungenerated;

                _ChunkEntities.Add(origin, chunk);
            }

            while (_DeactivationPendingChunks.TryPop(out Vector3 origin))
            {
                _ChunkEntities.Remove(origin, out IEntity? entity);

                if (entity is null)
                {
                    continue;
                }

                entityManager.RemoveEntity(entity);
            }
        }
    }
}
