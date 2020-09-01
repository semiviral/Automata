using System;
using System.Collections.Generic;

namespace Automata.Entity
{
    public interface IEntity
    {
        Guid ID { get; }
        Dictionary<Type, IComponent>.KeyCollection ComponentTypes { get; }

        bool TryAddComponent(IComponent component);
        bool TryRemoveComponent<T>() where T : IComponent;
        bool TryGetComponent<T>(out T component) where T : IComponent;
        T GetComponent<T>() where T : IComponent;
        IComponent GetComponent(Type componentType);

        int GetHashCode() => ID.GetHashCode();
    }
}
