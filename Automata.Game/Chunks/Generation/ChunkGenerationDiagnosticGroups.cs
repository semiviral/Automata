#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using DiagnosticsProviderNS;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public class BuildingTime : TimeSpanDiagnosticData
    {
        public BuildingTime(TimeSpan data) : base(data) { }
    }

    public class InsertionTime : TimeSpanDiagnosticData
    {
        public InsertionTime(TimeSpan data) : base(data) { }
    }

    public class MeshingTime : TimeSpanDiagnosticData
    {
        public MeshingTime(TimeSpan data) : base(data) { }
    }

    public class ApplyMeshTime : TimeSpanDiagnosticData
    {
        public ApplyMeshTime(TimeSpan data) : base(data) { }
    }

    public class ChunkGenerationDiagnosticGroup : IDiagnosticGroup
    {
        private readonly BoundedConcurrentQueue<ApplyMeshTime> _ApplyMeshTimes;
        private readonly BoundedConcurrentQueue<BuildingTime> _BuildingTimes;
        private readonly BoundedConcurrentQueue<InsertionTime> _InsertionTimes;
        private readonly BoundedConcurrentQueue<MeshingTime> _MeshingTimes;

        public IEnumerable<BuildingTime> BuildingTimes => _BuildingTimes;
        public IEnumerable<InsertionTime> InsertionTimes => _InsertionTimes;
        public IEnumerable<MeshingTime> MeshingTimes => _MeshingTimes;
        public IEnumerable<ApplyMeshTime> ApplyMeshTimes => _ApplyMeshTimes;

        public ChunkGenerationDiagnosticGroup()
        {
            _BuildingTimes = new BoundedConcurrentQueue<BuildingTime>(300);
            _InsertionTimes = new BoundedConcurrentQueue<InsertionTime>(300);
            _MeshingTimes = new BoundedConcurrentQueue<MeshingTime>(300);
            _ApplyMeshTimes = new BoundedConcurrentQueue<ApplyMeshTime>(300);
        }

        public override string ToString()
        {
            double buildingTime = BuildingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double insertionTimes = InsertionTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double meshingTime = MeshingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double applyMeshTime = ApplyMeshTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);

            return
                $"({nameof(BuildingTime)} {buildingTime:0.00}ms, {nameof(InsertionTime)} {insertionTimes:0.00}ms, {nameof(MeshingTime)} {meshingTime:0.00}ms, {nameof(ApplyMeshTime)} {applyMeshTime:0.00}ms)";
        }

        public void CommitData<TDataType>(IDiagnosticData<TDataType> data)
        {
            switch (data)
            {
                case BuildingTime buildingTime:
                    _BuildingTimes.Enqueue(buildingTime);
                    break;
                case InsertionTime insertionTime:
                    _InsertionTimes.Enqueue(insertionTime);
                    break;
                case MeshingTime meshingTime:
                    _MeshingTimes.Enqueue(meshingTime);
                    break;
                case ApplyMeshTime applyMeshTime:
                    _ApplyMeshTimes.Enqueue(applyMeshTime);
                    break;
                default: throw new ArgumentException("Data is not of a valid type for this diagnostic group.", nameof(data));
            }
        }
    }
}
