using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using DiagnosticsProviderNS;

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
            double building_time = BuildingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double insertion_times = InsertionTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double structures_times = StructuresTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double meshing_time = MeshingTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);
            double apply_mesh_time = ApplyMeshTimes.DefaultIfEmpty().Average(time => ((TimeSpan)time).TotalMilliseconds);

            return $"{nameof(BuildingTime)} {building_time:0.00}ms, "
                   + $"{nameof(InsertionTime)} {insertion_times:0.00}ms, "
                   + $"{nameof(StructuresTime)} {structures_times:0.00}ms, "
                   + $"{nameof(MeshingTime)} {meshing_time:0.00}ms, "
                   + $"{nameof(ApplyMeshTime)} {apply_mesh_time:0.00}ms";
        }

        public void CommitData<TDataType>(IDiagnosticData<TDataType> data)
        {
            switch (data)
            {
                case BuildingTime building_time:
                    _BuildingTimes.Enqueue(building_time);
                    break;
                case InsertionTime insertion_time:
                    _InsertionTimes.Enqueue(insertion_time);
                    break;
                case StructuresTime structures_time:
                    _StructuresTimes.Enqueue(structures_time);
                    break;
                case MeshingTime meshing_time:
                    _MeshingTimes.Enqueue(meshing_time);
                    break;
                case ApplyMeshTime apply_mesh_time:
                    _ApplyMeshTimes.Enqueue(apply_mesh_time);
                    break;
                default: throw new ArgumentException("Data is not of a valid type for this diagnostic group.", nameof(data));
            }
        }
    }
}
