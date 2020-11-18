using System.Diagnostics;
using Automata.Engine.Collections;

namespace Automata.Engine.Diagnostics
{
    public static class DiagnosticsPool
    {
        public static readonly ObjectPool<Stopwatch> Stopwatches = new ObjectPool<Stopwatch>(() => new Stopwatch());
    }
}
