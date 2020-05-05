#region

using System;
using System.Collections.Generic;

#endregion

namespace Automata.Core
{
    public interface IEntity
    {
        Guid ID { get; }

        T AddComponent<T>() where T : IComponent;
        void RemoveComponent<T>() where T : IComponent;
        T GetComponent<T>() where T : IComponent;

        bool TryAddComponent<T>() where T : IComponent;
        bool TryGetComponent<T>(out T component) where T : IComponent;
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

        public void AddComponent(IComponent component)
        {
            Type type = component.GetType();

            if (_Components.ContainsKey(type))
            {
                throw new Exception(ExceptionFormats.ComponentInstanceExistsException);
            }
        }

        public T AddComponent<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (_Components.ContainsKey(typeT))
            {
                throw new Exception(ExceptionFormats.ComponentInstanceExistsException);
            }

            T component = Activator.CreateInstance<T>();

            // todo fix boxing here
            _Components.Add(typeT, component);

            return component;
        }

        public bool TryAddComponent<T>() where T : IComponent => _Components.TryAdd(typeof(T), Activator.CreateInstance<T>());

        public void RemoveComponent<T>() where T : IComponent => _Components.Remove(typeof(T));

        public T GetComponent<T>() where T : IComponent
        {
            Type typeT = typeof(T);
            if (!_Components.TryGetValue(typeT, out IComponent? component) || (component == null))
            {
                throw new TypeLoadException(nameof(typeT));
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
