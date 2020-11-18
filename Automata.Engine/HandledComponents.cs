using System;

namespace Automata.Engine
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HandledComponents : Attribute
    {
        public ComponentTypes Types { get; }

        public HandledComponents(DistinctionStrategy strategy, params Type[] types) => Types = new ComponentTypes(strategy, types);
    }
}
