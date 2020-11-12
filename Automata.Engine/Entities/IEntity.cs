#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Entities
{
    public interface IEntity : IEquatable<IEntity>, ICollection<Component>
    {
        Guid ID { get; }
        bool Destroyed { get; }

        Component this[Type type] { get; init; }

        void Add<TComponent>() where TComponent : Component, new();
        TComponent Remove<TComponent>() where TComponent : Component;
        TComponent? Find<TComponent>() where TComponent : Component;
        bool Contains<TComponent>() where TComponent : Component;

        bool TryFind<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component;
        bool TryFind(Type type, [NotNullWhen(true)] out Component? component);

        internal void Destroy();

        int GetHashCode() => ID.GetHashCode();
    }
}
