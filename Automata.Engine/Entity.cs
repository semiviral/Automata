using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;

// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace Automata.Engine
{
    public enum ComponentError
    {
        Exists,
        NotExists
    }

    public sealed class Entity : IEquatable<Entity>, IDisposable
    {
        private readonly NonAllocatingList<Component> _Components;
        private readonly int _HashCode;

        public bool Disposed { get; private set; }

        public int Count => _Components.Count;

        public Component this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Components[index];
        }

        internal Entity()
        {
            _Components = new NonAllocatingList<Component>();
            _HashCode = Guid.NewGuid().GetHashCode();
            Disposed = false;
        }

        public override string ToString() =>
            $"{nameof(Entity)}(ID {_HashCode}: {string.Join(", ", _Components)})";


        #region Generic

        internal TComponent Add<TComponent>() where TComponent : Component, new()
        {
            if (Contains<TComponent>())
            {
                throw new InvalidOperationException("Component already exists.");
            }
            else
            {
                TComponent component = new TComponent();
                _Components.Add(component);
                return component;
            }
        }

        internal TComponent? Remove<TComponent>() where TComponent : Component
        {
            TComponent? result = Component<TComponent>();

            if (result is not null)
            {
                _Components.Remove(result);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TComponent? Component<TComponent>() where TComponent : Component
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
        public bool TryComponent<TComponent>([NotNullWhen(true)] out TComponent? result) where TComponent : Component
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

        internal Result<Component, ComponentError> Add(Component component)
        {
            if (Contains(component.GetType()))
            {
                return ComponentError.Exists;
            }
            else
            {
                _Components.Add(component);
                return component;
            }
        }

        internal Result<Component, ComponentError> Remove(Component component)
        {
            if (_Components.Remove(component))
            {
                component.Dispose();
                return component;
            }
            else
            {
                return ComponentError.NotExists;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Component? Component(Type type)
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
        public bool Contains(Type type) => Component(type) is not null;

        #endregion


        #region IEnumerable

        public NonAllocatingList<Component>.Enumerator GetEnumerator() => _Components.GetEnumerator();

        #endregion


        #region IEquatable

        public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);
        public bool Equals(Entity? other) => other is not null && _HashCode.Equals(other._HashCode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _HashCode;

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
