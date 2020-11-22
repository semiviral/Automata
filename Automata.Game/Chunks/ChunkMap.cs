using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Game.Blocks;
using Automata.Game.Chunks.Generation;

namespace Automata.Game.Chunks
{
    public class ChunkMap : IEnumerable<(Vector3i, Entity)>
    {
        private static readonly OcclusionBounds _ChunkOcclusionBounds = new OcclusionBounds
        {
            Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS),
            Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE))
        };

        private readonly Dictionary<Vector3i, Entity> _Chunks;
        private readonly ConcurrentChannel<(Vector3i, ChunkModification)>[] _PendingModifications;

        private int _PendingModificationIndex;

        public ICollection<Vector3i> Origins => _Chunks.Keys;
        public ICollection<Entity> Entities => _Chunks.Values;

        public ChunkMap()
        {
            _Chunks = new Dictionary<Vector3i, Entity>();

            _PendingModifications = new[]
            {
                new ConcurrentChannel<(Vector3i, ChunkModification)>(true, false),
                new ConcurrentChannel<(Vector3i, ChunkModification)>(true, false)
            };

            _PendingModificationIndex = 0;
        }

        public bool TryGetChunkEntity(Vector3i origin, [NotNullWhen(true)] out Entity? entity) => _Chunks.TryGetValue(origin, out entity);

        public bool TryGetBlockAt(Vector3i global, [MaybeNullWhen(false)] out Block block)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);
            Vector3i local = Vector3i.Abs(global - origin);
            int index = Vector3i.Project1D(local, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out Entity? entity) && entity.TryFind(out Chunk? chunk) && chunk.Blocks is not null)
            {
                block = chunk.Blocks[index];
                return true;
            }
            else
            {
                block = default;
                return false;
            }
        }

        public async ValueTask ProcessPendingChunkAllocations()
        {
            int nextIndex = (_PendingModificationIndex + 1) % _PendingModifications.Length;
            ConcurrentChannel<(Vector3i, ChunkModification)> channel = _PendingModifications[_PendingModificationIndex];

            while (channel.TryTake(out (Vector3i Origin, ChunkModification modification) pending))
            {
                if (_Chunks.TryGetValue(pending.Origin, out Entity? entity) && entity!.TryFind(out Chunk? chunk))
                {
                    await chunk.Modifications.AddAsync(pending.modification);
                }
                else
                {
                    await _PendingModifications[nextIndex].AddAsync(pending);
                }
            }

            Interlocked.Exchange(ref _PendingModificationIndex, nextIndex);
        }


        #region Chunk Addition / Removal

        public void Allocate(EntityManager entityManager, Vector3i origin)
        {
            if (!_Chunks.ContainsKey(origin))
            {
                _Chunks.Add(origin, entityManager.CreateEntity(
                    new Translation
                    {
                        Value = origin
                    }, new Chunk(),
                    _ChunkOcclusionBounds,
                    new RenderModel()
                ));
            }
        }

        public bool TryDeallocate(EntityManager entityManager, Vector3i origin, [NotNullWhen(true)] out Chunk? chunk)
        {
            if (_Chunks.Remove(origin, out Entity? entity) && entity is not null!)
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

            ChunkModification chunkModification = new ChunkModification
            {
                BlockIndex = Vector3i.Project1D(Vector3i.Abs(global - origin), GenerationConstants.CHUNK_SIZE),
                BlockID = blockID
            };

            if (_Chunks.TryGetValue(origin, out Entity? entity) && entity!.TryFind(out Chunk? chunk))
            {
                await chunk.Modifications.AddAsync(chunkModification);
            }
            else
            {
                await _PendingModifications[_PendingModificationIndex].AddAsync((origin, chunkModification));
            }
        }

        public async ValueTask AllocateChunkModifications(Vector3i global, IEnumerable<(Vector3i, ushort)> modifications)
        {
            foreach ((Vector3i local, ushort blockID) in modifications)
            {
                Vector3i modificationGlobal = global + local;
                Vector3i modificationOrigin = Vector3i.RoundBy(modificationGlobal, GenerationConstants.CHUNK_SIZE);

                if (_Chunks.TryGetValue(modificationOrigin, out Entity? entity) && entity!.TryFind(out Chunk? chunk))
                {
                    await chunk.Modifications.AddAsync(new ChunkModification
                    {
                        BlockIndex = Vector3i.Project1D(Vector3i.Abs(modificationGlobal - modificationOrigin), GenerationConstants.CHUNK_SIZE),
                        BlockID = blockID
                    });
                }
            }
        }

        #endregion


        #region IEnumerator

        public IEnumerator<(Vector3i, Entity)> GetEnumerator()
        {
            foreach ((Vector3i origin, var entity) in _Chunks)
            {
                yield return (origin, entity);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
