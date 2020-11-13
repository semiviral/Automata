#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public interface IEntity : ICollection<Component>, IEquatable<IEntity>, IDisposable
    {
        Guid ID { get; }
        bool Disposed { get; }

        Component? this[Type type] { get; init; }

        void Add<TComponent>() where TComponent : Component, new();
        TComponent Remove<TComponent>() where TComponent : Component;
        TComponent? Find<TComponent>() where TComponent : Component;
        bool Contains<TComponent>() where TComponent : Component;
        Component? Find(Type type);
        bool Contains(Type type);

        bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component;
        bool TryFind(Type type, [NotNullWhen(true)] out Component? component);

        int GetHashCode() => ID.GetHashCode();
    }
}
