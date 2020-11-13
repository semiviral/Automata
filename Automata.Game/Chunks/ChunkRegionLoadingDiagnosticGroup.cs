using System;
using System.Linq;
using Automata.Engine.Collections;
using DiagnosticsProviderNS;

namespace Automata.Game.Chunks
{
    public record RegionLoadingTime : TimeSpanDiagnosticData
    {
        public RegionLoadingTime(TimeSpan timeSpan) : base(timeSpan) { }
    }

    public sealed class ChunkRegionLoadingDiagnosticGroup : IDiagnosticGroup
    {
        private readonly BoundedConcurrentQueue<RegionLoadingTime> _RegionLoadingTimes;

        public ChunkRegionLoadingDiagnosticGroup() =>
            _RegionLoadingTimes = new BoundedConcurrentQueue<RegionLoadingTime>(Settings.Instance.DebugDataBufferSize);

        public void CommitData<TDataType>(IDiagnosticData<TDataType> data)
        {
            switch (data)
            {
                case RegionLoadingTime regionLoadingTime:
                    _RegionLoadingTimes.Enqueue(regionLoadingTime);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(data));
            }
        }

        public double Average() => _RegionLoadingTimes.DefaultIfEmpty().Average(time => time!.Data.TotalMilliseconds);
    }
}
