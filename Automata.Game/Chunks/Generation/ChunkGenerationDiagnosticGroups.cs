#region

using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using DiagnosticsProviderNS;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public record BuildingTime : TimeSpanDiagnosticData
    {
        public BuildingTime(TimeSpan data) : base(data) { }
    }

    public record InsertionTime : TimeSpanDiagnosticData
    {
        public InsertionTime(TimeSpan data) : base(data) { }
    }

    public record StructuresTime : TimeSpanDiagnosticData
    {
        public StructuresTime(TimeSpan data) : base(data) { }
    }

    public record MeshingTime : TimeSpanDiagnosticData
    {
        public MeshingTime(TimeSpan data) : base(data) { }
    }

    public record ApplyMeshTime : TimeSpanDiagnosticData
    {
        public ApplyMeshTime(TimeSpan data) : base(data) { }
    }

    public sealed class ChunkGenerationDiagnosticGroup : IDiagnosticGroup
    {
        private readonly BoundedConcurrentQueue<ApplyMeshTime> _ApplyMeshTimes;
        private readonly BoundedConcurrentQueue<BuildingTime> _BuildingTimes;
        private readonly BoundedConcurrentQueue<StructuresTime> _StructuresTimes;
        private readonly BoundedConcurrentQueue<InsertionTime> _InsertionTimes;
        private readonly BoundedConcurrentQueue<MeshingTime> _MeshingTimes;

        public IEnumerable<BuildingTime> BuildingTimes => _BuildingTimes;
        public IEnumerable<InsertionTime> InsertionTimes => _InsertionTimes;
        public IEnumerable<StructuresTime> StructuresTimes => _StructuresTimes;
        public IEnumerable<MeshingTime> MeshingTimes => _MeshingTimes;
        public IEnumerable<ApplyMeshTime> ApplyMeshTimes => _ApplyMeshTimes;

        public ChunkGenerationDiagnosticGroup()
        {
            int resolution = Settings.Instance.DebugDataBufferSize;
            _BuildingTimes = new BoundedConcurrentQueue<BuildingTime>(resolution);
            _InsertionTimes = new BoundedConcurrentQueue<InsertionTime>(resolution);
            _StructuresTimes = new BoundedConcurrentQueue<StructuresTime>(resolution);
            _MeshingTimes = new BoundedConcurrentQueue<MeshingTime>(resolution);
            _ApplyMeshTimes = new BoundedConcurrentQueue<ApplyMeshTime>(resolution);
        }

        public override string ToString()
        {
            double buildingTime = BuildingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double insertionTimes = InsertionTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double structuresTimes = StructuresTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double meshingTime = MeshingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double applyMeshTime = ApplyMeshTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);

            return $"{nameof(BuildingTime)} {buildingTime:0.00}ms, "
                   + $"{nameof(InsertionTime)} {insertionTimes:0.00}ms, "
                   + $"{nameof(StructuresTime)} {structuresTimes:0.00}ms, "
                   + $"{nameof(MeshingTime)} {meshingTime:0.00}ms, "
                   + $"{nameof(ApplyMeshTime)} {applyMeshTime:0.00}ms";
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
                case StructuresTime structuresTime:
                    _StructuresTimes.Enqueue(structuresTime);
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
