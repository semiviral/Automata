using System;
using System.Buffers;
using System.Collections.Generic;
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

        public bool Allocate(EntityManager entityManager, Vector3i origin)
        {
            if (_Chunks.ContainsKey(origin)) return false;
            else
            {
                IEntity chunk = new Entity
                {
                    new Chunk(),
                    _ChunkOcclusionBounds,
                    new Translation
                    {
                        Value = origin
                    }
                };

                entityManager.RegisterEntity(chunk);
                _Chunks.Add(origin, chunk);

                // update adjacent chunk meshes
                foreach (IEntity? entity in GetOriginNeighbors(origin).Where(entity => entity is not null))
                    if (entity!.TryFind(out Chunk? neighborChunk))
                        neighborChunk.State = (GenerationState)Math.Min((int)neighborChunk.State, (int)GenerationState.AwaitingMesh);

                return true;
            }
        }

        public bool Deallocate(EntityManager entityManager, Vector3i origin)
        {
            if (_Chunks.Remove(origin, out IEntity? chunk))
            {
                entityManager.RemoveEntity(chunk);
                return true;
            }
            else return false;
        }

        #endregion


        #region Chunk Modifications

        public async ValueTask AllocateChunkModification(Vector3i global, ushort blockID)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out IEntity? entity) && entity.TryFind(out Chunk? chunk))
                await chunk.Modifications.AddAsync(new ChunkModification
                {
                    Local = Vector3i.Abs(global - origin),
                    BlockID = blockID
                });
        }

        public async ValueTask AllocateChunkModifications(Vector3i global, IEnumerable<(Vector3i, ushort)> modifications)
        {
            foreach ((Vector3i local, ushort blockID) in modifications)
            {
                Vector3i modificationGlobal = global + local;
                Vector3i modificationOrigin = Vector3i.RoundBy(modificationGlobal, GenerationConstants.CHUNK_SIZE);

                if (_Chunks.TryGetValue(modificationOrigin, out IEntity? entity) && entity.TryFind(out Chunk? chunk))
                    await chunk.Modifications.AddAsync(new ChunkModification
                    {
                        Local = Vector3i.Abs(modificationGlobal - modificationOrigin),
                        BlockID = blockID
                    });
            }
        }

        #endregion


        public bool IsStateLockstep(GenerationState state, bool exact)
        {
            foreach (IEntity entity in Chunks)
                if (entity.TryFind(out Chunk? chunk)
                    && ((exact && (chunk.State != state)) || (!exact && (chunk.State < state))))
                    return false;

            return true;
        }

        public void RecalculateAllChunkNeighbors()
        {
            foreach ((Vector3i origin, IEntity entity) in _Chunks)
                if (entity.TryFind(out Chunk? chunk))
                {
                    int normalIndex = 0;

                    foreach (IEntity? neighbor in GetOriginNeighbors(origin))
                    {
                        chunk.Neighbors[normalIndex] = neighbor?.Find<Chunk>();
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
