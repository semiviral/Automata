#region

using System.Diagnostics;
using Automata.Engine.Collections;
using Automata.Engine.Systems;

#endregion


namespace Automata.Engine.Diagnostics
{
    public class DiagnosticsSystem : ComponentSystem
    {
        public static readonly ObjectPool<Stopwatch> Stopwatches = new ObjectPool<Stopwatch>(() => new Stopwatch());
    }
}
