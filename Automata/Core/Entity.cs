#region

using System;
using System.Collections.Generic;
using Automata.Core.Components;

#endregion

namespace Automata.Core
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

    public class Entity : IEntity
    {
        private readonly Dictionary<Type, IComponent> _Components;

        public Dictionary<Type, IComponent>.KeyCollection ComponentTypes => _Components.Keys;

        public Guid ID { get; }

        public Entity()
        {
            _Components = new Dictionary<Type, IComponent>();

            ID = Guid.NewGuid();
        }

        public bool TryAddComponent(IComponent? component)
        {
            if (component == null)
            {
                return false;
            }

            Type type = component.GetType();

            if (_Components.ContainsKey(type))
            {
                return false;
            }

            _Components.Add(type, component);
            return true;
        }

        public bool TryRemoveComponent<T>() where T : IComponent => _Components.Remove(typeof(T));

        public T GetComponent<T>() where T : IComponent
        {
            if (!_Components.TryGetValue(typeof(T), out IComponent? component))
            {
                throw new KeyNotFoundException(nameof(T));
            }
            else if (component == null)
            {
                throw new NullReferenceException(nameof(component));
            }
            else
            {
                return (T)component;
            }
        }

        public bool TryGetComponent<T>(out T component) where T : IComponent
        {
            if (_Components.TryGetValue(typeof(T), out IComponent? componentBase))
            {
                component = (T)componentBase;
                return true;
            }
            else
            {
                component = default!;
                return false;
            }
        }

        public IComponent GetComponent(Type componentType)
        {
            if (!typeof(IComponent).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"Type must be assignable from {nameof(IComponent)}.", nameof(componentType));
            }
            else if (!_Components.TryGetValue(componentType, out IComponent? component))
            {
                throw new KeyNotFoundException(nameof(componentType));
            }
            else if (component == null)
            {
                throw new NullReferenceException("Returned component is null.");
            }
            else
            {
                return component;
            }
        }
    }
}
