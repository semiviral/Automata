using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;

namespace Automata.Game
{
    public class VoxelWorld : World
    {
        private readonly Dictionary<Vector3<int>, Chunk> _Chunks;
        private readonly Dictionary<Vector3<int>, NonAllocatingList<ChunkModification>> _Modifications;
        private readonly ConcurrentChannel<(Vector3<int>, ChunkModification)> _ConcurrentModifications;

        public int ChunkCount => _Chunks.Count;

        public Chunk this[Vector3<int> origin] => _Chunks[origin];

        public VoxelWorld(bool active) : base(active)
        {
            _Chunks = new Dictionary<Vector3<int>, Chunk>();
            _Modifications = new Dictionary<Vector3<int>, NonAllocatingList<ChunkModification>>();
            _ConcurrentModifications = new ConcurrentChannel<(Vector3<int>, ChunkModification)>(true, false);
        }

        public bool TryGetChunk(Vector3<int> origin, [NotNullWhen(true)] out Chunk? chunk) => _Chunks.TryGetValue(origin, out chunk);

        public bool TryGetBlock(Vector3<int> global, [MaybeNullWhen(false)] out Block block)
        {
            Vector3<int> origin = Vector3<int>.RoundBy(global, GenerationConstants.CHUNK_SIZE);
            Vector3<int> local = Vector3<int>.Abs(global - origin);
            int index = Vector.Project1D(local, GenerationConstants.CHUNK_SIZE);

            if (_Chunks.TryGetValue(origin, out Chunk? chunk) && chunk?.Blocks is not null)
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

        public void TrimMemory() => _Chunks.TrimExcess();


        #region Chunk Addition / Removal

        public bool ChunkExists(Vector3<int> origin) => _Chunks.ContainsKey(origin);

        public async ValueTask<Chunk?> AllocateChunk(Vector3<int> origin)
        {
            // it's better to double-check the key here, as opposed to
            // instantiating the entity data every call of this method
            //
            // it's pretty likely this first check returns true, so we manage to avoid
            // a lot of unnecessary allocations.
            if (ChunkExists(origin))
            {
                return null;
            }
            else
            {
                Chunk chunk = new Chunk();
                _Chunks.Add(origin, chunk);

                // all of the relevant modifications are loaded into the chunk
                if (_Modifications.TryGetValue(origin, out NonAllocatingList<ChunkModification>? modifications))
                {
                    foreach (ChunkModification modification in modifications!)
                    {
                        await chunk.Modifications.AddAsync(modification);
                    }

                    modifications.Dispose();
                    _Modifications.Remove(origin);
                }

                return chunk;
            }
        }

        public bool DeallocateChunk(Vector3<int> origin) => _Chunks.Remove(origin);

        #endregion


        #region Chunk Modifications

        public async ValueTask AllocateChunkModification(Vector3<int> global, ushort blockID)
        {
            Vector3<int> origin = Vector3<int>.RoundBy(global, GenerationConstants.CHUNK_SIZE);

            ChunkModification modification = new ChunkModification
            {
                BlockIndex = Vector.Project1D(Vector3<int>.Abs(global - origin), GenerationConstants.CHUNK_SIZE),
                BlockID = blockID
            };

            await _ConcurrentModifications.AddAsync((origin, modification));
        }

        public async ValueTask AllocateChunkModifications(IEnumerable<(Vector3<int>, ushort)> modifications)
        {
            foreach ((Vector3<int> global, ushort blockID) in modifications)
            {
                await AllocateChunkModification(global, blockID);
            }
        }

        public async ValueTask SynchronizeConcurrentModifications()
        {
            while (_ConcurrentModifications.TryTake(out (Vector3<int> Origin, ChunkModification Modification) entry))
            {
                if (_Chunks.TryGetValue(entry.Origin, out Chunk? chunk))
                {
                    await chunk!.Modifications.AddAsync(entry.Modification);
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

        public Dictionary<Vector3<int>, Chunk>.Enumerator GetEnumerator() => _Chunks.GetEnumerator();

        #endregion
    }
}
