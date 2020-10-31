#region

using System;
using System.Collections;
using System.Collections.Generic;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Systems
{
    public enum DistinctionStrategy
    {
        None = 1,
        Any = 2,
        All = 3
    }

    public class ComponentTypes : IReadOnlyCollection<Type>, IEquatable<ComponentTypes>
    {
        public static readonly ComponentTypes Empty = new ComponentTypes(DistinctionStrategy.Any);
        private readonly int _CompositeHashCode;

        private readonly HashSet<Type> _Types;

        public DistinctionStrategy Strategy { get; }

        public ComponentTypes(DistinctionStrategy strategy, params Type[] types)
        {
            Strategy = strategy;

            _Types = new HashSet<Type>();

            int hashCode = 17;

            foreach (Type type in types)
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new TypeLoadException($"All given types must implement {typeof(IComponent)}.");
                }
                else if (!_Types.Add(type))
                {
                    throw new ArgumentException($"Cannot specify the same {nameof(IComponent)} type more than once.");
                }
                else
                {
                    const int prime = 239;

                    hashCode *= prime + type.GetHashCode();
                }
            }

            _CompositeHashCode = hashCode;
        }

        public override int GetHashCode() => _CompositeHashCode;

        public int Count => _Types.Count;


        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => _Types.GetEnumerator();
        public IEnumerator<Type> GetEnumerator() => ((IEnumerable<Type>)_Types).GetEnumerator();

        #endregion


        #region IEquatable

        public bool Equals(ComponentTypes? other) => _CompositeHashCode == other?._CompositeHashCode;

        public override bool Equals(object? obj) => obj is ComponentTypes other && Equals(other);

        public static bool operator ==(ComponentTypes left, ComponentTypes right) => left.Equals(right);

        public static bool operator !=(ComponentTypes left, ComponentTypes right) => !left.Equals(right);

        #endregion
    }
}
