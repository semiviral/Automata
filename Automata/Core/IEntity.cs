#region

using System;

#endregion

namespace Automata.Core
{
    public interface IEntity
    {
        Guid ID { get; }

        T AddComponent<T>() where T : IComponent;
        T GetComponent<T>() where T : IComponent;

        bool TryAddComponent<T>() where T : IComponent;
    }
}
