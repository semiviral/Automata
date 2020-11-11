using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Game.Chunks.Generation;

namespace Automata.Game.Chunks
{
    public class ChunkMap
    {
        private static readonly OcclusionBounds _ChunkOcclusionBounds = new OcclusionBounds
        {
            Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS),
            Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE))
        };

        private readonly Dictionary<Vector3i, IEntity> _Chunks;

        public ICollection<Vector3i> Origins => _Chunks.Keys;
        public ICollection<IEntity> Chunks => _Chunks.Values;

        public ChunkMap() => _Chunks = new Dictionary<Vector3i, IEntity>();


        #region Chunk Addition / Removal

        public bool TryAdd(EntityManager entityManager, Vector3i origin, [NotNullWhen(true)] out IEntity? chunk)
        {
            static IEntity ComposeChunkImpl(Vector3i origin)
            {
                IEntity chunk = new Entity();
                chunk.AddComponent<Chunk>();
                chunk.AddComponent(_ChunkOcclusionBounds);

                chunk.AddComponent(new Translation
                {
                    Value = origin
                });

                return chunk;
            }

            if (_Chunks.ContainsKey(origin))
            {
                chunk = null;
                return false;
            }
            else
            {
                chunk = ComposeChunkImpl(origin);
                entityManager.RegisterEntity(chunk);
                _Chunks.Add(origin, chunk);

                // update adjacent chunk meshes
                foreach (IEntity? entity in GetOriginNeighbors(origin).Where(entity => entity is not null))
                {
                    Chunk neighborChunk = entity!.GetComponent<Chunk>();
                    neighborChunk.State = (GenerationState)Math.Min((int)neighborChunk.State, (int)GenerationState.GenerateMesh);
                }

                return true;
            }
        }

        public bool TryRemove(EntityManager entityManager, Vector3i origin, out IEntity? chunks)
        {
            if (_Chunks.Remove(origin, out chunks))
            {
                entityManager.RemoveEntity(chunks);
                return true;
            }
            else return false;
        }

        #endregion


        #region Chunk Modifications

        public async ValueTask AllocateChunkModification(Vector3i global, ushort blockID)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out IEntity? entity) && entity.TryGetComponent(out Chunk? chunk))
                await chunk.Modifications.AddAsync(new ChunkModification
                {
                    Local = global - origin,
                    BlockID = blockID
                });
        }

        public async ValueTask AllocateChunkModifications(Vector3i global, IEnumerable<(Vector3i, ushort)> modifications)
        {
            foreach ((Vector3i local, ushort blockID) in modifications)
            {
                Vector3i modificationGlobal = global + local;
                Vector3i modificationOrigin = Vector3i.RoundBy(modificationGlobal, GenerationConstants.CHUNK_SIZE);

                if (_Chunks.TryGetValue(modificationOrigin, out IEntity? entity) && entity.TryGetComponent(out Chunk? chunk))
                    await chunk.Modifications.AddAsync(new ChunkModification
                    {
                        Local = Vector3i.Abs(modificationGlobal - modificationOrigin),
                        BlockID = blockID
                    });
            }
        }

        #endregion


        public void RecalculateAllChunkNeighbors()
        {
            foreach ((Vector3i origin, IEntity entity) in _Chunks)
            {
                Chunk chunk = entity.GetComponent<Chunk>();
                int normalIndex = 0;

                foreach (IEntity? neighbor in GetOriginNeighbors(origin))
                {
                    chunk.Neighbors[normalIndex] = neighbor?.GetComponent<Chunk>();
                    normalIndex += 1;
                }
            }
        }

        private IEnumerable<IEntity?> GetOriginNeighbors(Vector3i origin)
        {
            for (int normalIndex = 0; normalIndex < 6; normalIndex++)
            {
                int sign = (normalIndex - 3) >= 0 ? -1 : 1;
                int componentIndex = normalIndex % 3;
                Vector3i component = Vector3i.One.WithComponent<Vector3i, int>(componentIndex) * sign;
                Vector3i neighborOrigin = origin + (component * GenerationConstants.CHUNK_SIZE);

                _Chunks.TryGetValue(neighborOrigin, out IEntity? neighbor);
                yield return neighbor;
            }
        }
    }
}
