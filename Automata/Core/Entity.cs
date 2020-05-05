#region

using System;
using System.Collections.Generic;

#endregion

namespace Automata.Core
{
    public interface IEntity
    {
        Guid ID { get; }

        bool TryAddComponent(IComponent component);
        bool TryRemoveComponent<T>() where T : IComponent;
        bool TryGetComponent<T>(out T component) where T : IComponent;
        T GetComponent<T>() where T : IComponent;
    }

    public class Entity : IEntity
    {
        private readonly Dictionary<Type, IComponent> _Components;

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
            Type typeT = typeof(T);

            if (!_Components.TryGetValue(typeT, out IComponent? component) || (component == null))
            {
                throw new TypeLoadException(typeT.ToString());
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
                component = (T)Component.Default;
                return false;
            }
        }
    }
}
