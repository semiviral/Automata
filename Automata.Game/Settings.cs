using System;
using Automata.Engine;

namespace Automata.Game
{
    public class Settings : Singleton<Settings>
    {
        public bool SingleThreadedGeneration { get; }
        public TimeSpan AcceptableUpdateTimeSlice { get; } = TimeSpan.FromMilliseconds(1d);
    }
}
