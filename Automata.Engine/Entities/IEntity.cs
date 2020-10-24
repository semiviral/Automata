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
        IReadOnlyDictionary<Type, IComponent> Components { get; }

        void AddComponent(IComponent component);

        TComponent RemoveComponent<TComponent>() where TComponent : class, IComponent;
        IComponent RemoveComponent(Type type);

        TComponent GetComponent<TComponent>() where TComponent : class, IComponent;
        IComponent GetComponent(Type componentType);
        bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : class, IComponent;
        bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component);

        public bool HasComponent<TComponent>() where TComponent : class, IComponent;

        int GetHashCode() => ID.GetHashCode();
    }
}
