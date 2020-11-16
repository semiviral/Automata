#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public interface IEntity : IEnumerable<Component>, IEquatable<IEntity>, IDisposable
    {
        Guid ID { get; }
        bool Disposed { get; }
        int Count { get; }

        Component this[int index] { get; }

        internal void Add<TComponent>() where TComponent : Component, new();
        internal TComponent Remove<TComponent>() where TComponent : Component;
        TComponent? Find<TComponent>() where TComponent : Component;
        bool Contains<TComponent>() where TComponent : Component;

        internal void Add(Component component);
        internal bool Remove(Component component);
        Component? Find(Type type);
        bool Contains(Type type);

        bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? result) where TComponent : Component;
    }
}
