using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

namespace Automata.Engine
{
    public interface IEntity : IEquatable<IEntity>, IDisposable
    {
        Guid ID { get; }
        bool Disposed { get; }
        int Count { get; }

        Component this[int index] { get; }
        Component this[uint index] { get; }

        internal TComponent Add<TComponent>() where TComponent : Component, new();
        internal TComponent Remove<TComponent>() where TComponent : Component;
        TComponent? Find<TComponent>() where TComponent : Component;
        bool Contains<TComponent>() where TComponent : Component;
        bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? result) where TComponent : Component;

        internal void Add(Component component);
        internal bool Remove(Component component);
        Component? Find(Type type);
        bool Contains(Type type);

        Enumerator GetEnumerator();

        public struct Enumerator : IEnumerator<Component>
        {
            private readonly Entity _Entity;

            private uint _Index;
            private Component? _Current;

            public Component Current => _Current!;

            object? IEnumerator.Current
            {
                get
                {
                    if ((_Index == 0u) || (_Index >= (uint)_Entity.Count))
                    {
                        ThrowHelper.ThrowInvalidOperationException("Enumerable has not been enumerated.");
                    }

                    return _Current;
                }
            }

            internal Enumerator(Entity entity)
            {
                _Entity = entity;
                _Index = 0u;
                _Current = default;
            }

            public bool MoveNext()
            {
                if (_Index >= (uint)_Entity.Count)
                {
                    return false;
                }

                _Current = _Entity[_Index];
                _Index += 1u;
                return true;
            }

            void IEnumerator.Reset()
            {
                _Index = 0u;
                _Current = default;
            }


            #region IDisposable

            public void Dispose() { }

            #endregion
        }
    }
}
