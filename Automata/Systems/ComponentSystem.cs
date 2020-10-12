#region

using System;
using Automata.Entities;

#endregion

namespace Automata.Systems
{
    public abstract class ComponentSystem : IDisposable
    {
        protected EntityManager? _EntityManager;

        /// <summary>
        ///     This is a hint to the <see cref="SystemManager" /> as to what types the system uses for updates.
        ///     If the types aren't present in the <see cref="EntityManager" />, the system's <see cref="Update" /> will be
        ///     skipped.
        /// </summary>
        public ComponentTypes? HandledComponents { get; protected set; }

        public bool Enabled { get; protected set; }

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered() { }

        public ComponentSystem() => Enabled = true;

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        /// <remarks>
        ///     If none of the types in <see cref="HandledComponents" /> are active in the <see cref="EntityManager" />, this
        ///     method is skipped.
        /// </remarks>
        public virtual void Update(EntityManager entityManager, TimeSpan delta) { }

        #region IDisposable

        private bool _Disposed;

        protected virtual void DisposeInternal() { }

        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            DisposeInternal();
            GC.SuppressFinalize(this);
            _Disposed = true;
        }

        #endregion
    }
}
