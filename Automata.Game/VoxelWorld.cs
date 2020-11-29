using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        private static readonly OcclusionBounds _ChunkOcclusionBounds = new OcclusionBounds
        {
            Spheric = new Sphere(new Vector3(GenerationConstants.CHUNK_RADIUS), GenerationConstants.CHUNK_RADIUS),
            Cubic = new Cube(Vector3.Zero, new Vector3(GenerationConstants.CHUNK_SIZE))
        };

        private readonly Dictionary<Vector3i, Entity> _Chunks;
        private readonly Dictionary<Vector3i, NonAllocatingList<ChunkModification>> _Modifications;
        private readonly ConcurrentChannel<(Vector3i, ChunkModification)> _ConcurrentModificationsQueue;

        public ICollection<Vector3i> Origins => _Chunks.Keys;
        public ICollection<Entity> Entities => _Chunks.Values;
        public int ChunkCount => _Chunks.Count;

        public Entity this[Vector3i origin] => _Chunks[origin];

        public VoxelWorld(bool active) : base(active)
        {
            _Chunks = new Dictionary<Vector3i, Entity>();
            _Modifications = new Dictionary<Vector3i, NonAllocatingList<ChunkModification>>();
            _ConcurrentModificationsQueue = new ConcurrentChannel<(Vector3i, ChunkModification)>(true, false);
        }

        public bool TryGetChunkEntity(Vector3i origin, [NotNullWhen(true)] out Entity? entity) => _Chunks.TryGetValue(origin, out entity);

        public bool TryGetBlockAt(Vector3i global, [MaybeNullWhen(false)] out Block block)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);
            Vector3i local = Vector3i.Abs(global - origin);
            int index = Vector3i.Project1D(local, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out Entity? entity) && entity!.TryComponent(out Chunk? chunk) && chunk?.Blocks is not null)
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


        #region Chunk Addition / Removal

        public async ValueTask TryAllocate(EntityManager entityManager, Vector3i origin)
        {
            // it's better to double-check the key here, as opposed to
            // instantiating the entity data every call of this method
            //
            // it's pretty likely this first check returns true, so we manage to avoid
            // a lot of unnecessary allocations.
            if (!_Chunks.ContainsKey(origin))
            {
                Chunk chunk = new Chunk();

                _Chunks.Add(origin, entityManager.CreateEntity(
                    new Transform
                    {
                        Translation = origin
                    },
                    chunk,
                    _ChunkOcclusionBounds
                ));

                // here all of the relevant modifications are loaded into the chunk
                if (_Modifications.TryGetValue(origin, out NonAllocatingList<ChunkModification>? modifications))
                {
                    foreach (ChunkModification modification in modifications!)
                    {
                        await chunk.Modifications.AddAsync(modification);
                    }

                    modifications.Dispose();
                    _Modifications.Remove(origin);
                }
            }
        }

        public bool TryDeallocate(Vector3i origin) => _Chunks.Remove(origin);

        #endregion


        #region Chunk Modifications

        public async ValueTask AllocateChunkModification(Vector3i global, ushort blockID)
        {
            Vector3i origin = Vector3i.RoundBy(global, GenerationConstants.CHUNK_SIZE);

            ChunkModification modification = new ChunkModification
            {
                BlockIndex = Vector3i.Project1D(Vector3i.Abs(global - origin), GenerationConstants.CHUNK_SIZE),
                BlockID = blockID
            };

            await _ConcurrentModificationsQueue.AddAsync((origin, modification));
        }

        public async ValueTask AllocateChunkModifications(IEnumerable<(Vector3i, ushort)> modifications)
        {
            foreach ((Vector3i global, ushort blockID) in modifications)
            {
                await AllocateChunkModification(global, blockID);
            }
        }

        public async ValueTask ProcessConcurrentModifications()
        {
            while (_ConcurrentModificationsQueue.TryTake(out (Vector3i Origin, ChunkModification Modification) entry))
            {
                if (_Chunks.TryGetValue(entry.Origin, out Entity? entity))
                {
                    await entity!.Component<Chunk>()!.Modifications.AddAsync(entry.Modification);
                }
                else
                {
                    _Modifications.TryAdd(entry.Origin, new NonAllocatingList<ChunkModification>());
                    _Modifications[entry.Origin].Add(entry.Modification);
                }
            }
        }

        #endregion


        #region IEnumerator

        public Dictionary<Vector3i, Entity>.Enumerator GetEnumerator() => _Chunks.GetEnumerator();

        #endregion
    }
}
