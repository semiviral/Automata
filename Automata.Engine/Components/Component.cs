using System;

namespace Automata.Engine.Components
{
    public abstract class Component : IEquatable<Component>
    {
        public Guid ID { get; }

        public Component() => ID = Guid.NewGuid();

        public bool Equals(Component? other) => other is not null && ID.Equals(other.ID);
        public override bool Equals(object? obj) => obj is Component component && Equals(component);

        public override int GetHashCode() => ID.GetHashCode();

        public static bool operator ==(Component? left, Component? right) => Equals(left, right);
        public static bool operator !=(Component? left, Component? right) => !Equals(left, right);
    }

    public abstract class ComponentChangeable : Component
    {
        public bool Changed { get; set; }
    }
}
