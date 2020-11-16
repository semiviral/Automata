using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Game.Chunks.Generation;

namespace Automata.Game.Chunks
{
    public class ChunkMap : IEnumerable<(Vector3i, IEntity)>
    {
        private static readonly OcclusionBounds _ChunkOcclusionBounds = new OcclusionBounds
        {
            Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS),
            Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE))
        };

        private readonly Dictionary<Vector3i, IEntity> _Chunks;

        public ICollection<Vector3i> Origins => _Chunks.Keys;
        public ICollection<IEntity> Entities => _Chunks.Values;

        public ChunkMap() => _Chunks = new Dictionary<Vector3i, IEntity>();

        public bool TryGetChunkEntity(Vector3i origin, [NotNullWhen(true)] out IEntity? entity) => _Chunks.TryGetValue(origin, out entity);


        #region Chunk Addition / Removal

        public bool TryAllocate(EntityManager entityManager, Vector3i origin, [NotNullWhen(true)] out Chunk? chunk)
        {
            if (_Chunks.ContainsKey(origin))
            {
                chunk = null;
                return false;
            }
            else
            {
                _Chunks.Add(origin, entityManager.CreateEntity(
                    chunk = new Chunk(),
                    new Translation
                    {
                        Value = origin
                    },
                    _ChunkOcclusionBounds,
                    new RenderModel()
                ));

                return true;
            }
        }

        public bool TryDeallocate(EntityManager entityManager, Vector3i origin, [NotNullWhen(true)] out Chunk? chunk)
        {
            if (_Chunks.Remove(origin, out IEntity? entity) && entity is not null!)
            {
                bool success = entity.TryFind(out chunk);
                entityManager.RemoveEntity(entity);
                return success;
            }
            else
            {
                chunk = null;
                return false;
            }
        }

        #endregion


        #region Chunk Modifications

        public async ValueTask AllocateChunkModification(Vector3i global, ushort blockID)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out IEntity? entity) && entity.TryFind(out Chunk? chunk))
                await chunk.Modifications.AddAsync(new ChunkModification
                {
                    BlockIndex = Vector3i.Project1D(Vector3i.Abs(global - origin), GenerationConstants.CHUNK_SIZE),
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
                        BlockIndex = Vector3i.Project1D(Vector3i.Abs(modificationGlobal - modificationOrigin), GenerationConstants.CHUNK_SIZE),
                        BlockID = blockID
                    });
            }
        }

        #endregion


        #region IEnumerator

        public IEnumerator<(Vector3i, IEntity)> GetEnumerator()
        {
            foreach ((Vector3i origin, var entity) in _Chunks) yield return (origin, entity);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
