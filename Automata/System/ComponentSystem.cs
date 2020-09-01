#region

using System;
using Automata.Entity;

#endregion

namespace Automata.System
{
    public abstract class ComponentSystem
    {
        /// <summary>
        ///     This is a hint to the <see cref="SystemManager" /> as to what types the system uses for updates.
        ///     If the types aren't present in the <see cref="EntityManager" />, the system's <see cref="Update" /> will be
        ///     skipped.
        /// </summary>
        public ComponentTypes HandledComponentTypes { get; protected set; }

        public bool Enabled { get; protected set; }

        public ComponentSystem()
        {
            HandledComponentTypes = new ComponentTypes();
            Enabled = true;
        }

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered() { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        /// <remarks>
        ///     If none of the types in <see cref="HandledComponentTypes" /> are active in the <see cref="EntityManager" />, this
        ///     method is skipped.
        /// </remarks>
        public virtual void Update(EntityManager entityManager, TimeSpan delta) { }

        /// <summary>
        ///     Method called when the system is destroyed.
        /// </summary>
        public virtual void Destroy(EntityManager entityManager) { }
    }
}
