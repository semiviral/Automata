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
        int ID { get; }
        bool Disposed { get; }

        void Add<TComponent>() where TComponent : Component, new();
        TComponent Remove<TComponent>() where TComponent : Component;
        TComponent? Find<TComponent>() where TComponent : Component;
        bool Contains<TComponent>() where TComponent : Component;

        void Add(Component component);
        Component? Find(Type type);
        bool Contains(Type type);

        bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component;

        int GetHashCode() => ID.GetHashCode();
    }
}
