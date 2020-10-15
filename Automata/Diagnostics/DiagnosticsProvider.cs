#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Automata.Collections;
using Serilog;

#endregion

namespace Automata.Diagnostics
{
    public interface IDiagnosticData<out TDataType>
    {
        public TDataType Data { get; }
    }

    public interface IDiagnosticGroup
    {
        public void CommitData<TDataType>(IDiagnosticData<TDataType> data);
    }

    public abstract class TimeSpanDiagnosticData : IDiagnosticData<TimeSpan>
    {
        public TimeSpan Data { get; }

        protected TimeSpanDiagnosticData(TimeSpan data) => Data = data;

        public static explicit operator TimeSpan(TimeSpanDiagnosticData? a) => a?.Data ?? TimeSpan.Zero;
    }

    public static class DiagnosticsProvider
    {
        private static readonly Dictionary<Type, IDiagnosticGroup> _EnabledGroups;

        public static readonly ObjectPool<Stopwatch> Stopwatches = new ObjectPool<Stopwatch>(() => new Stopwatch());

        /// <summary>
        ///     Determines whether errors are emitted by the default logger when a particular <see cref="IDiagnosticGroup" /> has
        ///     not been enabled.
        /// </summary>
        public static bool EmitNotEnabledErrors { get; set; }

        static DiagnosticsProvider() => _EnabledGroups = new Dictionary<Type, IDiagnosticGroup>();

        /// <summary>
        ///     Enables given <see cref="TDiagnosticGroup" /> for logging data.
        /// </summary>
        /// <typeparam name="TDiagnosticGroup"><see cref="IDiagnosticGroup" /> to enable for data logging.</typeparam>
        public static void EnableGroup<TDiagnosticGroup>() where TDiagnosticGroup : class, IDiagnosticGroup, new()
        {
            if (_EnabledGroups.ContainsKey(typeof(TDiagnosticGroup)))
            {
                Log.Error($"Diagnostic group '{typeof(TDiagnosticGroup).FullName}' is already enabled.");
            }
            else
            {
                TDiagnosticGroup diagnosticGroup = new TDiagnosticGroup();
                _EnabledGroups.Add(typeof(TDiagnosticGroup), diagnosticGroup);
            }
        }

        public static TDiagnosticGroup GetGroup<TDiagnosticGroup>() where TDiagnosticGroup : class, IDiagnosticGroup, new() =>
            (TDiagnosticGroup)_EnabledGroups[typeof(TDiagnosticGroup)];

        /// <summary>
        ///     Commits <see cref="IDiagnosticData{T}" /> to the given <see cref="IDiagnosticGroup" /> of type
        ///     <see cref="TDiagnosticGroup" />.
        /// </summary>
        public static void CommitData<TDiagnosticGroup, TDataType>(IDiagnosticData<TDataType> diagnosticData)
            where TDiagnosticGroup : IDiagnosticGroup
        {
            if (_EnabledGroups.TryGetValue(typeof(TDiagnosticGroup), out IDiagnosticGroup? diagnosticGroup))
            {
                diagnosticGroup.CommitData(diagnosticData);
            }
            else if (EmitNotEnabledErrors)
            {
                Log.Error($"Diagnostic group '{typeof(TDiagnosticGroup).FullName}' has not been enabled. Please enable before commiting data.");
            }
        }
    }
}
