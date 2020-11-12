#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public class Entity : IEntity
    {
        private readonly List<Component> _Components;

        public Guid ID { get; }
        public bool Destroyed { get; private set; }

        public Component? this[Type type]
        {
            get => _Components.Find(type.IsInstanceOfType);
            init
            {
                if (value is null) throw new NullReferenceException("Value cannot be null.");

                int index = _Components.FindIndex(type.IsInstanceOfType);

                if (index > -1) _Components[index] = value;
                else _Components.Add(value);
            }
        }

        public int Count => _Components.Count;

        public Entity()
        {
            _Components = new List<Component>();

            ID = Guid.NewGuid();
            Destroyed = false;
        }

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

        public TComponent? Find<TComponent>() where TComponent : Component => _Components.Find(typeof(TComponent).IsInstanceOfType) as TComponent;

        public bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component
        {
            component = this[typeof(TComponent)] as TComponent;
            return component is not null;
        }

        public bool TryFind(Type type, [NotNullWhen(true)] out Component? component)
        {
            component = this[type];
            return component is not null;
        }

        public bool Contains<TComponent>() where TComponent : Component => this[typeof(TComponent)] is not null;
        public bool Contains(Type type) => this[type] is not null;

        void IEntity.Destroy() => Destroyed = true;

        public bool Contains(Component item) => _Components.IndexOf(item) > 0;
        public void CopyTo(Component[] array, int arrayIndex) => _Components.CopyTo(array, arrayIndex);


        #region ICollection

        bool ICollection<Component>.IsReadOnly => (_Components as ICollection<Component>).IsReadOnly;

        public void Add(Component component)
        {
            if (Contains(component.GetType())) throw new ArgumentException("Already contains component of given type.");
            else _Components.Add(component);
        }

        public bool Remove(Component item) => _Components.Remove(item);

        void ICollection<Component>.Clear() => _Components.Clear();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Component> GetEnumerator() => _Components.GetEnumerator();

        #endregion


        #region IEquatable

        public override bool Equals(object? obj) => obj is IEntity entity && Equals(entity);
        public bool Equals(IEntity? other) => other is not null && ID.Equals(other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
        public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);

        #endregion
    }
}
