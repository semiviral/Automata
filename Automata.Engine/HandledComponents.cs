using System;
using System.Collections;
using System.Collections.Generic;

namespace Automata.Engine
{
    public enum EnumerationStrategy
    {
        Any,
        All,
        None
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HandledComponents : Attribute, IEnumerable<Type>, IEquatable<HandledComponents>
    {
        private readonly int _CompositeHashCode;
        private readonly Type[] _Types;

        public EnumerationStrategy Strategy { get; }

        public HandledComponents(EnumerationStrategy strategy, params Type[] types)
        {
            Strategy = strategy;

            HashSet<Type> processed = new();

            int hashCode = 17;

            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(Component)))
                {
                    throw new TypeLoadException($"All given types must implement {typeof(Component)}.");
                }
                else if (!processed.Add(type))
                {
                    throw new ArgumentException($"Cannot specify the same {nameof(Component)} type more than once.");
                }
                else
                {
                    hashCode = HashCode.Combine(hashCode, type.GetHashCode());
                }
            }

            _Types = types;
            _CompositeHashCode = hashCode;
        }


        #region IEnumerable

        public IEnumerator<Type> GetEnumerator() => (_Types as IEnumerable<Type>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion


        #region IEquatable

        public bool Equals(HandledComponents? other) => _CompositeHashCode == other?._CompositeHashCode;
        public override bool Equals(object? obj) => obj is HandledComponents other && Equals(other);

        public override int GetHashCode() => _CompositeHashCode;

        public static bool operator ==(HandledComponents left, HandledComponents right) => left.Equals(right);
        public static bool operator !=(HandledComponents left, HandledComponents right) => !left.Equals(right);

        #endregion
    }
}
