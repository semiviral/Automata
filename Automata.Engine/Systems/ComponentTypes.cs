#region

using System;
using System.Collections;
using System.Collections.Generic;
using Automata.Engine.Components;

#endregion

namespace Automata.Engine.Systems
{
    public class ComponentTypes : IEnumerable<Type>, IEquatable<ComponentTypes>
    {
        private readonly HashSet<Type> _Types;
        private readonly int _CompositeHashCode;

        public IReadOnlyCollection<Type> Types => _Types;

        public ComponentTypes(params Type[] types)
        {
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
