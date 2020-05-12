using System;
using System.Collections.Generic;
using Automata.Collections;

namespace Automata
{
    public class Diagnostics : Singleton<Diagnostics>
    {
        public Dictionary<string, FixedConcurrentQueue<TimeSpan>> _DiagnosticTimes;

        public Diagnostics()
        {
            _DiagnosticTimes = new Dictionary<string, FixedConcurrentQueue<TimeSpan>>();
        }

        public void Register
    }
}
