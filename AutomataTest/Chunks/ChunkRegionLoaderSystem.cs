using System;
using System.Collections.Generic;
using System.Numerics;
using Automata;
using Automata.Components;
using Automata.Entities;
using Automata.Numerics;
using Automata.Systems;
using AutomataTest.Chunks.Generation;
using Serilog;

namespace AutomataTest.Chunks
{
    public class ChunkRegionLoaderSystem : ComponentSystem
    {
        private Dictionary<Vector3, IEntity> _ChunkEntities;

        private HashSet<Vector3> _DeactivatingChunks;
        private HashSet<Vector3> _ActivatingChunks;

        public ChunkRegionLoaderSystem()
        {
            HandledComponents = new ComponentTypes(typeof(Translation), typeof(ChunkLoader));

            _ChunkEntities = new Dictionary<Vector3, IEntity>();
        }

        private static bool IsWithinLoaderRange(Vector3i difference, ChunkLoader chunkLoader) =>
            Vector3b.All(difference <= (GenerationConstants.CHUNK_SIZE * chunkLoader.Radius));

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach ((Translation translation, ChunkLoader chunkLoader) in entityManager.GetComponents<Translation, ChunkLoader>())
            {
                if (translation.Changed)
                {


                    Vector3 roundedTranslation = translation.Value.RoundBy(new Vector3(GenerationConstants.CHUNK_SIZE));

                    // allocate list of chunks requiring deactivation
                    foreach ((Vector3 origin, IEntity _) in _ChunkEntities)
                    {
                        Vector3 differenceF = Vector3.Abs(origin - roundedTranslation);
                        Vector3i difference = new Vector3i((int)differenceF.X, 0, (int)differenceF.Z);

                        if (IsWithinLoaderRange(difference, chunkLoader))
                        {
                            _DeactivatingChunks.Remove(origin);
                        }
                        else
                        {
                            _DeactivatingChunks.Add(origin);
                        }
                    }

                    for (int y = -chunkLoader.Radius; y < GenerationConstants.WORLD_HEIGHT_IN_CHUNKS; y++)
                    for (int z = -chunkLoader.Radius; z < chunkLoader.Radius + 1; z++)
                    for (int x = -chunkLoader.Radius; x < chunkLoader.Radius + 1; x++)
                    {
                        Vector3 localOrigin = new Vector3(x, y, z) * GenerationConstants.CHUNK_SIZE;
                        Vector3 globalOrigin = localOrigin + new Vector3(roundedTranslation.X, 0f, roundedTranslation.Z);
                    }
                }
            }

                        _Stopwatch.Restart();

            WorldState |= WorldState.VerifyingState;

            // get total list of out of bounds chunks
            foreach (IEntity loader in _EntityLoaders)
            {
                // allocate list of chunks requiring deactivation
                foreach ((float3 origin, ChunkController _) in _Chunks)
                {
                    float3 difference = math.abs(origin - loader.ChunkPosition);
                    difference.y = 0; // always load all chunks on y axis

                    if (!IsWithinLoaderRange(difference))
                    {
                        _ChunksRequiringDeactivation.Add(origin);
                    }
                    else
                    {
                        _ChunksRequiringDeactivation.Remove(origin);
                    }
                }

                // todo this should be some setting inside loader
                int renderRadius = Options.Instance.RenderDistance;

                for (int x = -renderRadius; x < (renderRadius + 1); x++)
                for (int z = -renderRadius; z < (renderRadius + 1); z++)
                for (int y = 0; y < WORLD_HEIGHT_IN_CHUNKS; y++)
                {
                    float3 localOrigin = new float3(x, y, z) * GenerationConstants.CHUNK_SIZE;
                    float3 globalOrigin = localOrigin + new float3(loader.ChunkPosition.x, 0, loader.ChunkPosition.z);

                    _ChunksRequiringActivation.Add(globalOrigin);
                }
            }

            Log.Debug(
                $"({nameof(WorldController)}) State verification: {_ChunksRequiringActivation.Count} activations, {_ChunksRequiringDeactivation.Count} deactivations.");

            foreach (float3 origin in _ChunksRequiringActivation.Where(origin => !CheckChunkExists(origin)))
            {
                _ChunksPendingActivation.Push(origin);
            }

            foreach (float3 origin in _ChunksRequiringDeactivation)
            {
                _ChunksPendingDeactivation.Push(origin);
            }

            _ChunksRequiringActivation.Clear();
            _ChunksRequiringDeactivation.Clear();

            WorldState &= ~WorldState.VerifyingState;

            Singletons.Diagnostics.Instance["WorldStateVerification"].Enqueue(_Stopwatch.Elapsed);

            _Stopwatch.Reset();
        }
    }
}
