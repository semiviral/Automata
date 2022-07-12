using System;
using System.Linq;
using Automata.Engine.Collections;
using DiagnosticsProviderNS;

namespace Automata.Game.Chunks
{
    public record ChunkModificationTime : TimeSpanDiagnosticData
    {
        public ChunkModificationTime(TimeSpan timeSpan) : base(timeSpan) { }
    }

    public sealed class ChunkModificationsDiagnosticGroup : IDiagnosticGroup
    {
        private readonly BoundedConcurrentQueue<ChunkModificationTime> _ChunkModificationTimes;

        public ChunkModificationsDiagnosticGroup() =>
            _ChunkModificationTimes = new BoundedConcurrentQueue<ChunkModificationTime>(Settings.Instance.DebugDataBufferSize);

        public void CommitData<TDataType>(IDiagnosticData<TDataType> data)
        {
            switch (data)
            {
                case ChunkModificationTime chunk_modification_time:
                    _ChunkModificationTimes.Enqueue(chunk_modification_time);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(data));
            }
        }

        public double Average() => _ChunkModificationTimes.Count > 0
            ? _ChunkModificationTimes.DefaultIfEmpty().Average(time => time?.Data.TotalMilliseconds ?? throw new NullReferenceException(nameof(time)))
            : 0d;
    }
}
