using System;

namespace Automata.Engine.Systems
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlesComponents : Attribute
    {
        public ComponentTypes Types { get; }

        public HandlesComponents(DistinctionStrategy strategy, params Type[] types) => Types = new ComponentTypes(strategy, types);
    }
}
