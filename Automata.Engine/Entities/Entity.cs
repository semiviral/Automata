using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Collections;
using Automata.Engine.Components;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Automata.Engine.Entities
{
    internal sealed class Entity : IEntity
    {
        private readonly NonAllocatingList<Component> _Components;

        public int ID { get; }
        public bool Disposed { get; private set; }

        internal Entity(int id)
        {
            ID = id;
            Disposed = false;
            _Components = new NonAllocatingList<Component>(0);
        }


        #region Generic

        public void Add<TComponent>() where TComponent : Component, new()
        {
            if (Contains<TComponent>()) ThrowHelper.ThrowArgumentException(typeof(TComponent).Name, "Entity already has component of type.");
            else _Components.Add(new TComponent());
        }

        public TComponent Remove<TComponent>() where TComponent : Component
        {
            TComponent? component = Find<TComponent>();

            if (component is null)
            {
                ThrowHelper.ThrowArgumentException(typeof(TComponent).Name, "Entity does not have component of type.");
                return null!;
            }

            _Components.Remove(component);
            return component;
        }

        public TComponent? Find<TComponent>() where TComponent : Component
        {
            foreach (Component component in _Components)
                if (component is TComponent componentT)
                    return componentT;

            return null;
        }

        public bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component
        {
            foreach (Component current in _Components)
                if (current is TComponent result)
                {
                    component = result;
                    return true;
                }

            component = null;
            return false;
        }

        public bool Contains<TComponent>() where TComponent : Component
        {
            foreach (Component component in _Components)
                if (component is TComponent)
                    return true;

            return false;
        }

        #endregion


        #region Non-generic

        public void Add(Component component)
        {
            if (Contains(component.GetType())) ThrowHelper.ThrowArgumentException(component.GetType().Name, "Entity already contains component of type.");
            else _Components.Add(component);
        }

        public Component? Find(Type type)
        {
            foreach (Component component in _Components)
                if (type.IsInstanceOfType(component))
                    return component;

            return null;
        }

        public bool Contains(Type type) => Find(type) is not null;

        #endregion


        #region IEnumerable

        public IEnumerator<Component> GetEnumerator() => _Components.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion


        #region IEquatable

        public override bool Equals(object? obj) => obj is IEntity entity && Equals(entity);
        public bool Equals(IEntity? other) => other is not null && ID.Equals(other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (Disposed) return;

            foreach (Component component in _Components)
                if (component is IDisposable disposable)
                    disposable.Dispose();

            _Components.Dispose();

            Disposed = true;
        }

        #endregion
    }
}
