#region

using System.Diagnostics;
using Automata.Collections;
using Automata.Systems;

#endregion

namespace Automata.Diagnostics
{
    public class DiagnosticsSystem : ComponentSystem
    {
        public static readonly ObjectPool<Stopwatch> Stopwatches = new ObjectPool<Stopwatch>(() => new Stopwatch());

        public DiagnosticsSystem() => HandledComponents = new ComponentTypes();
    }
}
