#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Automata.Collections;
using Automata.Jobs;
using Automata.Numerics;
using AutomataTest.Blocks;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public abstract class ChunkTerrainJob : AsyncParallelJob
    {
        private static readonly ConcurrentDictionary<string, ushort> _BlockIDCache = new ConcurrentDictionary<string, ushort>();

        protected readonly Stopwatch Stopwatch;

        protected Vector3i _OriginPoint;
        protected Random? _SeededRandom;
        protected INodeCollection<ushort>? _Blocks;

        protected ChunkTerrainJob() : base(GenerationConstants.CHUNK_SIZE_CUBED, 256) => Stopwatch = new Stopwatch();

        protected void SetData(Vector3i originPoint)
        {
            CancellationToken = AsyncJobScheduler.AbortToken;
            _OriginPoint = originPoint;
            _SeededRandom = new Random(_OriginPoint.GetHashCode());
        }

        protected static ushort GetCachedBlockID(string blockName)
        {
            if (_BlockIDCache.TryGetValue(blockName, out ushort id))
            {
                return id;
            }
            else if (BlockRegistry.Instance.TryGetBlockId(blockName, out id))
            {
                _BlockIDCache.TryAdd(blockName, id);
                return id;
            }

            throw new ArgumentException("Block does not exist.", nameof(blockName));
        }

        public INodeCollection<ushort> GetGeneratedBlockData()
        {
            if (_Blocks == null)
            {
                throw new NullReferenceException("Blocks collection has not been built.");
            }
            else
            {
                return _Blocks;
            }
        }
    }
}
