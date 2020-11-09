#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public class Entity : IEntity
    {
        private readonly List<Component> _Components;

        public Guid ID { get; }
        public bool Destroyed { get; private set; }
        public IEnumerable<Component> Components => _Components;

        public Entity()
        {
            _Components = new List<Component>();

            ID = Guid.NewGuid();
            Destroyed = false;
        }

        public void AddComponent(Component component)
        {
            if (HasComponent(component.GetType())) throw new ArgumentException("Already contains component of given type.");
            else _Components.Add(component);
        }

        public void AddComponent<TComponent>() where TComponent : Component, new()
        {
            if (HasComponent<TComponent>()) throw new ArgumentException("Already contains component of given type.");
            else _Components.Add(new TComponent());
        }

        public TComponent RemoveComponent<TComponent>() where TComponent : Component
        {
            TComponent component = GetComponent<TComponent>();
            _Components.Remove(component);
            return component;
        }

        public Component RemoveComponent(Type type)
        {
            Component component = GetComponent(type);
            _Components.Remove(component);
            return component;
        }

        public TComponent GetComponent<TComponent>() where TComponent : Component
        {
            foreach (Component component in _Components)
                if (component is TComponent tComponent)
                    return tComponent;

            throw new ArgumentException("Entity does not have a component of given type.");
        }

        public Component GetComponent(Type type)
        {
            foreach (Component component in _Components.Where(type.IsInstanceOfType)) return component;

            throw new ArgumentException("Entity does not have a component of given type.");
        }

        public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component
        {
            foreach (Component component1 in _Components)
            {
                if (component1 is not TComponent tComponent) continue;

                component = tComponent;
                return true;
            }

            component = null;
            return false;
        }

        public bool TryGetComponent(Type type, [NotNullWhen(true)] out Component? component)
        {
            foreach (Component component1 in _Components.Where(type.IsInstanceOfType))
            {
                component = component1;
                return true;
            }

            component = null;
            return false;
        }

        public bool HasComponent<TComponent>() where TComponent : Component => _Components.Any(component => component is TComponent);
        public bool HasComponent(Type type) => _Components.Any(type.IsInstanceOfType);

        void IEntity.Destroy() => Destroyed = true;

        public override bool Equals(object? obj) => obj is IEntity entity && Equals(entity);
        public bool Equals(IEntity? other) => other is not null && ID.Equals(other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
        public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
    }
}
