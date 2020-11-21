using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automata.Engine.Collections;

// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace Automata.Engine
{
    internal sealed class Entity : IEntity
    {
        private readonly NonAllocatingList<Component> _Components;

        public Guid ID { get; }
        public bool Disposed { get; private set; }

        public Component this[int index] => _Components[index];
        public Component this[uint index] => _Components[index];

        public Entity()
        {
            ID = Guid.NewGuid();
            Disposed = false;
            _Components = new NonAllocatingList<Component>();
        }

        public int Count => _Components.Count;


        #region Generic

        TComponent IEntity.Add<TComponent>()
        {
            if (Contains<TComponent>())
            {
                ThrowHelper.ThrowArgumentException(typeof(TComponent).Name, "Entity already has component of type.");
                return null!;
            }
            else
            {
                TComponent component = new TComponent();
                _Components.Add(component);
                return component;
            }
        }

        TComponent IEntity.Remove<TComponent>()
        {
            TComponent? component = Find<TComponent>();

            switch (component)
            {
                case null:
                    ThrowHelper.ThrowArgumentException(typeof(TComponent).Name, "Entity does not have component of type.");
                    return null!;
                case { } when _Components.Remove(component):
                    component.Dispose();
                    break;
            }

            return component;
        }

        public TComponent? Find<TComponent>() where TComponent : Component
        {
            foreach (Component component in _Components)
            {
                if (component is TComponent componentT)
                {
                    return componentT;
                }
            }

            return null;
        }

        public bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? result) where TComponent : Component
        {
            foreach (Component component in _Components)
            {
                if (component is TComponent componentT)
                {
                    result = componentT;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public bool Contains<TComponent>() where TComponent : Component
        {
            foreach (Component component in _Components)
            {
                if (component is TComponent)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Non-generic

        void IEntity.Add(Component component)
        {
            if (Contains(component.GetType()))
            {
                ThrowHelper.ThrowArgumentException(component.GetType().Name, "Entity already contains component of type.");
            }
            else
            {
                _Components.Add(component);
            }
        }

        bool IEntity.Remove(Component component)
        {
            bool success = _Components.Remove(component);

            if (success)
            {
                component.Dispose();
            }

            return success;
        }

        public Component? Find(Type type)
        {
            foreach (Component component in _Components)
            {
                if (type.IsInstanceOfType(component))
                {
                    return component;
                }
            }

            return null;
        }

        public bool Contains(Type type) => Find(type) is not null;

        #endregion


        #region IEnumerable

        public IEntity.Enumerator GetEnumerator() => new IEntity.Enumerator(this);

        #endregion


        #region IEquatable

        public override bool Equals(object? obj) => obj is IEntity entity && Equals(entity);
        public bool Equals(IEntity? other) => other is not null && ID.Equals(other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        public override string ToString() => $"{nameof(Entity)}(ID {ID}: {string.Join(", ", _Components.Select(component => component.GetType().Name))})";

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            foreach (Component component in _Components)
            {
                component.Dispose();
            }

            _Components.Dispose();

            Disposed = true;
        }

        #endregion
    }
}
