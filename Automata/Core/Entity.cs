#region

using System;
using System.Collections.Generic;

#endregion

namespace Automata.Core
{
    public class Entity : IEntity
    {
        private readonly Dictionary<Type, IComponent> _Components;

        public Guid ID { get; }

        public Entity()
        {
            _Components = new Dictionary<Type, IComponent>();

            ID = Guid.NewGuid();
        }

        public T AddComponent<T>() where T : IComponent
        {
            Type typeT = typeof(T);

            if (_Components.ContainsKey(typeT))
            {
                throw new Exception($"Entity already contains component ({typeT}).");
            }

            T component = Activator.CreateInstance<T>();

            // todo fix boxing here
            _Components.Add(typeT, component);

            return component;
        }

        public bool TryAddComponent<T>() where T : IComponent => _Components.TryAdd(typeof(T), Activator.CreateInstance<T>());

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
