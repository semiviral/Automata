using System;
using System.Collections.Generic;
using Automata.Collections;

namespace Automata.Singletons
{
    public class Diagnostics : Singleton<Diagnostics>
    {
        private readonly Dictionary<string, FixedConcurrentQueue<TimeSpan>> _DiagnosticTimes;

        public FixedConcurrentQueue<TimeSpan> this[string name]
        {
            get
            {
                if (!_DiagnosticTimes.ContainsKey(name))
                {
                    throw new KeyNotFoundException(name);
                }
                else
                {
                    return _DiagnosticTimes[name];
                }
            }
        }

        public Diagnostics()
        {
            AssignSingletonInstance(this);

            _DiagnosticTimes = new Dictionary<string, FixedConcurrentQueue<TimeSpan>>();
        }

        public void RegisterDiagnosticTimeEntry(string name)
        {
            if (_DiagnosticTimes.ContainsKey(name))
            {
                throw new ArgumentException("Diagnostic entry already exists.", nameof(name));
            }
            else
            {
                _DiagnosticTimes.Add(name, new FixedConcurrentQueue<TimeSpan>(300));
            }
        }

        public void UnregisterDiagnosticTimeEntry(string name)
        {
            if (!_DiagnosticTimes.ContainsKey(name))
            {
                throw new ArgumentException("Diagnostic entry does not exists.", nameof(name));
            }
            else
            {
                _DiagnosticTimes.Remove(name);
            }
        }
    }
}
