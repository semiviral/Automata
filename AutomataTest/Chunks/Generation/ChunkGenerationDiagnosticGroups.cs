#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Automata.Diagnostics;

#endregion

namespace AutomataTest.Chunks.Generation
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
        private readonly ConcurrentBag<BuildingTime> _BuildingTimes;
        private readonly ConcurrentBag<InsertionTime> _InsertionTimes;
        private readonly ConcurrentBag<MeshingTime> _MeshingTimes;
        private readonly ConcurrentBag<ApplyMeshTime> _ApplyMeshTimes;

        public IEnumerable<BuildingTime> BuildingTimes => _BuildingTimes;
        public IEnumerable<InsertionTime> InsertionTimes => _InsertionTimes;
        public IEnumerable<MeshingTime> MeshingTimes => _MeshingTimes;
        public IEnumerable<ApplyMeshTime> ApplyMeshTimes => _ApplyMeshTimes;

        public ChunkGenerationDiagnosticGroup()
        {
            _BuildingTimes = new ConcurrentBag<BuildingTime>();
            _InsertionTimes = new ConcurrentBag<InsertionTime>();
            _MeshingTimes = new ConcurrentBag<MeshingTime>();
            _ApplyMeshTimes = new ConcurrentBag<ApplyMeshTime>();
        }

        public void CommitData(IDiagnosticData data)
        {
            switch (data)
            {
                case BuildingTime buildingTime:
                    _BuildingTimes.Add(buildingTime);
                    break;
                case InsertionTime insertionTime:
                    _InsertionTimes.Add(insertionTime);
                    break;
                case MeshingTime meshingTime:
                    _MeshingTimes.Add(meshingTime);
                    break;
                case ApplyMeshTime applyMeshTime:
                    _ApplyMeshTimes.Add(applyMeshTime);
                    break;
                default:
                    throw new ArgumentException("Data is not of a valid type for this diagnostic group.", nameof(data));
            }
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
    }
}
