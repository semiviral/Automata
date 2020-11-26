using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;

// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace Automata.Engine
{
    public sealed class Entity : IEquatable<Entity>, IDisposable
    {
        private readonly NonAllocatingList<Component> _Components;

        public Guid ID { get; }
        public bool Disposed { get; private set; }

        public int Count => _Components.Count;

        public Component this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Components[index];
        }

        public Entity()
        {
            ID = Guid.NewGuid();
            Disposed = false;
            _Components = new NonAllocatingList<Component>();
        }


        #region Generic

        internal TComponent Add<TComponent>() where TComponent : Component, new()
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

        internal TComponent Remove<TComponent>() where TComponent : Component
        {
            if (TryFind(out TComponent? component) && _Components.Remove(component))
            {
                component.Dispose();
                return component;
            }
            else
            {
                ThrowHelper.ThrowArgumentException(typeof(TComponent).Name, "Entity does not have component of type.");
                return null!;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TComponent? Find<TComponent>() where TComponent : Component
        {
            for (int index = 0; index < _Components.Count; index++)
            {
                if (_Components[index] is TComponent component)
                {
                    return component;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? result) where TComponent : Component
        {
            for (int index = 0; index < _Components.Count; index++)
            {
                if (_Components[index] is TComponent component)
                {
                    result = component;
                    return true;
                }
            }

            result = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<TComponent>() where TComponent : Component
        {
            for (int index = 0; index < _Components.Count; index++)
            {
                if (_Components[index] is TComponent)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Non-generic

        internal void Add(Component component)
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

        internal bool Remove(Component component)
        {
            bool success = _Components.Remove(component);

            if (success)
            {
                component.Dispose();
            }

            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Type type) => Find(type) is not null;

        #endregion


        #region IEnumerable

        public NonAllocatingList<Component>.Enumerator GetEnumerator() => _Components.GetEnumerator();

        #endregion


        #region IEquatable

        public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);
        public bool Equals(Entity? other) => other is not null && ID.Equals(other.ID);

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
