#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Automata.System
{
    public class ComponentTypes : IEquatable<ComponentTypes>
    {
        private readonly Type[] _Types;
        private readonly int _CompositeHashCode;

        public IReadOnlyList<Type> Types => _Types;

        public ComponentTypes(params Type[] types)
        {
            int hashCode = 17;

            if (types.Length > 0)
            {
                if (types.Any(type => !typeof(IComponent).IsAssignableFrom(type)))
                {
                    throw new Exception($"All given types for group must implement {typeof(IComponent)}.");
                }
                else
                {
                    const int prime = 239;

                    // calculate compound hash
                    foreach (Type type in types)
                    {
                        hashCode *= prime + type.GetHashCode();
                    }
                }
            }

            _Types = types;
            _CompositeHashCode = hashCode;
        }

        public override int GetHashCode() => _CompositeHashCode;

        public bool Equals(ComponentTypes? other) => _CompositeHashCode == other?._CompositeHashCode;

        public override bool Equals(object? obj) => obj is ComponentTypes other && Equals(other);

        public static bool operator ==(ComponentTypes left, ComponentTypes right) => left.Equals(right);

        public static bool operator !=(ComponentTypes left, ComponentTypes right) => !left.Equals(right);
    }
}
