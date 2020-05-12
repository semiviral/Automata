#region

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Automata.Collections;
using Automata.Jobs;
using Random = System.Random;

#endregion

namespace AutomataTest
{
    public abstract class ChunkTerrainJob : AsyncParallelJob
    {
        private static readonly ConcurrentDictionary<string, ushort> _BlockIDCache = new ConcurrentDictionary<string, ushort>();

        protected readonly Stopwatch Stopwatch;

        protected Vector3 _OriginPoint;
        protected Random _SeededRandom;
        protected INodeCollection<ushort> _Blocks;

        protected ChunkTerrainJob() : base(GenerationConstants.CHUNK_SIZE_CUBED, 256) => Stopwatch = new Stopwatch();

        protected void SetData(CancellationToken cancellationToken, Vector3 originPoint)
        {
            CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(AsyncJobScheduler.AbortToken, cancellationToken).Token;
            _OriginPoint = originPoint;

            _SeededRandom = new Random(_OriginPoint.GetHashCode());
        }

        protected static ushort GetCachedBlockID(string blockName)
        {
            if (_BlockIDCache.TryGetValue(blockName, out ushort id))
            {
                return id;
            }
            else if (BlockController.Current.TryGetBlockId(blockName, out id))
            {
                _BlockIDCache.TryAdd(blockName, id);
                return id;
            }

            throw new ArgumentException("Block with given name does not exist.", nameof(blockName));
        }

        public INodeCollection<ushort> GetGeneratedBlockData() => _Blocks;
    }
}
