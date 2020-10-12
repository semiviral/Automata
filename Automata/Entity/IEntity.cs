#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Automata.Entity
{
    public interface IEntity
    {
        Guid ID { get; }
        Dictionary<Type, IComponent>.KeyCollection ComponentTypes { get; }

        void AddComponent(IComponent component);
        void RemoveComponent<T>() where T : IComponent;
        T GetComponent<T>() where T : IComponent;

        bool TryGetComponent<T>(out T component) where T : IComponent;
        IComponent GetComponent(Type componentType);

        int GetHashCode() => ID.GetHashCode();
    }
}
