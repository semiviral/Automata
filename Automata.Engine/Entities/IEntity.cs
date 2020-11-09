#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public interface IEntity : IEquatable<IEntity>
    {
        Guid ID { get; }
        IEnumerable<Component> Components { get; }

        void AddComponent(Component component);
        void AddComponent<TComponent>() where TComponent : Component, new();

        TComponent RemoveComponent<TComponent>() where TComponent : Component;
        Component RemoveComponent(Type type);

        TComponent GetComponent<TComponent>() where TComponent : Component;
        Component GetComponent(Type type);
        bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component;
        bool TryGetComponent(Type type, [NotNullWhen(true)] out Component? component);

        public bool HasComponent<TComponent>() where TComponent : Component;

        int GetHashCode() => ID.GetHashCode();
    }
}
