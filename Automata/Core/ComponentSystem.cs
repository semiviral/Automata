#region

using System;

#endregion

namespace Automata.Core
{
    public enum SystemType
    {
        /// <summary>
        ///     System executes computational work.
        /// </summary>
        Computation,

        /// <summary>
        ///     System executes rendering work (i.e. draw calls).
        /// </summary>
        Rendering
    }

    public abstract class ComponentSystem
    {
        /// <summary>
        ///     The <see cref="SystemManager" /> uses this boolean to track whether it has enabled the given System.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        ///     This is a hint to the <see cref="SystemManager" /> as to what types the system uses for updates.
        ///     If the types aren't present in the <see cref="EntityManager" />, the system's <see cref="Update" /> will be
        ///     skipped.
        /// </summary>
        public Type[] UtilizedComponentTypes { get; protected set; } = new Type[0];

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered() { }

        /// <summary>
        ///     Method called when the <see cref="ComponentSystem" /> is enabled.
        /// </summary>
        /// <remarks>
        ///     Enabling happens before the first execution of <see cref="Update" />.
        /// </remarks>
        public virtual void Enabled() { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        /// <remarks>
        ///     If none of the types in <see cref="UtilizedComponentTypes" /> are active in the <see cref="EntityManager" />, this
        ///     method is skipped.
        /// </remarks>
        public virtual void Update() { }
    }
}
