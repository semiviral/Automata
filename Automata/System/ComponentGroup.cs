#region

using System;
using System.Linq;

#endregion

namespace Automata.System
{
    public readonly struct ComponentGroup : IEquatable<ComponentGroup>
    {
        private readonly int _CachedHashCode;

        public ComponentGroup(params Type[] groupedTypes)
        {
            if (groupedTypes.Length == 0)
            {
                throw new Exception("Given array must have length greater than 0.");
            }
            else if (groupedTypes.Any(type => !typeof(IComponent).IsAssignableFrom(type)))
            {
                throw new Exception($"All given types for group must implement {typeof(IComponent)}.");
            }

            _CachedHashCode = groupedTypes[0].GetHashCode();

            foreach (Type type in groupedTypes.Skip(1))
            {
                _CachedHashCode ^= type.GetHashCode();
            }
        }

        public override int GetHashCode() => _CachedHashCode;

        public bool Equals(ComponentGroup other) => _CachedHashCode == other._CachedHashCode;

        public override bool Equals(object? obj) => obj is ComponentGroup other && Equals(other);

        public static bool operator ==(ComponentGroup left, ComponentGroup right) => left.Equals(right);

        public static bool operator !=(ComponentGroup left, ComponentGroup right) => !left.Equals(right);
    }
}
