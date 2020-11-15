using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Collections;
using Automata.Engine.Components;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace Automata.Engine.Entities
{
    public class Entity : IEntity
    {
        private readonly NonAllocatingList<Component> _Components;

        public Guid ID { get; }
        public bool Disposed { get; private set; }

        public Component? this[Type type]
        {
            get => Find(type);
            init
            {
                static int FindIndex(IEnumerable<Component> components, Type type)
                {
                    int index = 0;

                    foreach (Component component in components)
                        if (type.IsInstanceOfType(component)) return index;
                        else index += 1;

                    return -1;
                }

                if (value is null) ThrowHelper.ThrowNullReferenceException("Value cannot be null.");

                int index = FindIndex(_Components, type);

                if (index > -1) _Components[index] = value!;
                else _Components.Add(value!);
            }
        }

        public int Count => _Components.Count;

        public Entity() => (ID, Disposed, _Components) = (Guid.NewGuid(), false, new NonAllocatingList<Component>(1));

        public void Add<TComponent>() where TComponent : Component, new()
        {
            if (Contains<TComponent>()) throw new ArgumentException("Already contains component of given type.");
            else _Components.Add(new TComponent());
        }

        public TComponent Remove<TComponent>() where TComponent : Component
        {
            TComponent component = this[typeof(TComponent)] as TComponent ?? throw new ArgumentException("Entity does not have component of type.");
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

        public Component? Find(Type type)
        {
            foreach (Component component in _Components)
                if (type.IsInstanceOfType(component))
                    return component;

            return null;
        }

        public bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component
        {
            foreach (Component component1 in _Components)
                if (component1 is TComponent componentT)
                {
                    component = componentT;
                    return true;
                }

            component = null;
            return false;
        }

        public bool TryFind(Type type, [NotNullWhen(true)] out Component? component)
        {
            foreach (Component component1 in _Components)
                if (type.IsInstanceOfType(component1))
                {
                    component = component1;
                    return true;
                }

            component = null;
            return false;
        }

        public bool Contains<TComponent>() where TComponent : Component
        {
            foreach (Component component1 in _Components)
                if (component1 is TComponent)
                    return true;

            return false;
        }

        public bool Contains(Type type) => this[type] is not null;

        public void CopyTo(Component[] array, int arrayIndex) => _Components.CopyTo(array, arrayIndex);


        #region ICollection

        bool ICollection<Component>.IsReadOnly => (_Components as ICollection<Component>).IsReadOnly;

        public void Add(Component component)
        {
            if (Contains(component.GetType())) throw new ArgumentException("Already contains component of given type.");
            else _Components.Add(component);
        }

        public bool Remove(Component item) => _Components.Remove(item);

        bool ICollection<Component>.Contains(Component item) => _Components.IndexOf(item) > 0;
        void ICollection<Component>.Clear() => _Components.Clear();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Component> GetEnumerator() => _Components.GetEnumerator();

        #endregion


        #region IEquatable

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
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
