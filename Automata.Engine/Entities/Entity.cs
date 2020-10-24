#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public class Entity : IEntity
    {
        private readonly Dictionary<Type, IComponent> _Components;

        public Entity()
        {
            _Components = new Dictionary<Type, IComponent>();

            ID = Guid.NewGuid();
        }

        public override bool Equals(object? obj) => obj is IEntity entity && Equals(entity);

        public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

        public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);

        public Guid ID { get; }
        public IReadOnlyDictionary<Type, IComponent> Components => _Components;

        public void AddComponent(IComponent component) => _Components.Add(component.GetType(), component);

        public T RemoveComponent<T>() where T : class, IComponent
        {
            T component = (T)_Components[typeof(T)];
            _Components.Remove(typeof(T));
            return component;
        }

        public IComponent RemoveComponent(Type type)
        {
            IComponent component = _Components[type];
            _Components.Remove(type);
            return component;
        }

        public T GetComponent<T>() where T : class, IComponent => (T)_Components[typeof(T)];

        public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class, IComponent
        {
            if (_Components.TryGetValue(typeof(T), out IComponent? componentBase))
            {
                component = (T)componentBase;
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }

        public bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component) => _Components.TryGetValue(type, out component);

        public IComponent GetComponent(Type componentType)
        {
            if (!typeof(IComponent).IsAssignableFrom(componentType))
                throw new ArgumentException($"Type must be assignable from {nameof(IComponent)}.", nameof(componentType));

            return _Components[componentType];
        }

        public bool HasComponent<T>() where T : class, IComponent => _Components.ContainsKey(typeof(T));

        public bool Equals(IEntity? other) => other is not null && ID.Equals(other.ID);

        public override int GetHashCode() => ID.GetHashCode();
    }
}
