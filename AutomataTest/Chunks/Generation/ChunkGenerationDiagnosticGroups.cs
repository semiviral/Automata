#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class BuildingTime : TimeSpanDiagnosticData
    {
        public BuildingTime(TimeSpan data) : base(data) { }
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
        private readonly List<BuildingTime> _BuildingTimes;
        private readonly List<MeshingTime> _MeshingTimes;
        private readonly List<ApplyMeshTime> _ApplyMeshTimes;

        public IReadOnlyList<BuildingTime> BuildingTimes => _BuildingTimes;
        public IReadOnlyList<MeshingTime> MeshingTimes => _MeshingTimes;
        public IReadOnlyList<ApplyMeshTime> ApplyMeshTimes => _ApplyMeshTimes;

        public ChunkGenerationDiagnosticGroup()
        {
            _BuildingTimes = new List<BuildingTime>();
            _MeshingTimes = new List<MeshingTime>();
            _ApplyMeshTimes = new List<ApplyMeshTime>();
        }

        public void CommitData(IDiagnosticData data)
        {
            switch (data)
            {
                case BuildingTime buildingTime:
                    _BuildingTimes.Add(buildingTime);
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
    }
}
