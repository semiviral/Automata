#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Automata.Entity
{
    public class Entity : IEntity
    {
        private readonly Dictionary<Type, IComponent> _Components;

        public Entity()
        {
            _Components = new Dictionary<Type, IComponent>();

            ID = Guid.NewGuid();
        }

        public Dictionary<Type, IComponent>.KeyCollection ComponentTypes => _Components.Keys;

        public Guid ID { get; }

        public void AddComponent(IComponent component) => _Components.Add(component.GetType(), component);

        public void RemoveComponent<T>() where T : IComponent => _Components.Remove(typeof(T));

        public T GetComponent<T>() where T : IComponent => (T)_Components[typeof(T)];

        public bool TryGetComponent<T>([NotNullWhen(true)] out T component) where T : IComponent
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

            return _Components[componentType];
        }
    }
}
