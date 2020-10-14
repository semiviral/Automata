#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class BuildingTime : IDiagnosticData
    {
        private object _Data;
        public object Data => _Data;

        public BuildingTime(TimeSpan data) => _Data = data;

        public static explicit operator TimeSpan(BuildingTime buildingTime) => Unsafe.As<object, TimeSpan>(ref buildingTime._Data);
    }

    public class MeshingTime : IDiagnosticData
    {
        private object _Data;
        public object Data => _Data;

        public MeshingTime(TimeSpan data) => _Data = data;

        public static explicit operator TimeSpan(MeshingTime meshingTime) => Unsafe.As<object, TimeSpan>(ref meshingTime._Data);
    }

    public class ChunkGenerationDiagnosticGroup : IDiagnosticGroup
    {
        private readonly List<BuildingTime> _BuildingTimes;
        private readonly List<MeshingTime> _MeshingTimes;

        public IReadOnlyList<BuildingTime> BuildingTimes => _BuildingTimes;
        public IReadOnlyList<MeshingTime> MeshingTimes => _MeshingTimes;

        public ChunkGenerationDiagnosticGroup()
        {
            _BuildingTimes = new List<BuildingTime>();
            _MeshingTimes = new List<MeshingTime>();
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
                default:
                    throw new ArgumentException("Data is not of a valid type for this diagnostic group.", nameof(data));
            }
        }
    }
}
