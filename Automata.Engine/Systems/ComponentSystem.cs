#region

using System;
using Automata.Engine.Entities;

#endregion

namespace Automata.Engine.Systems
{
    public abstract class ComponentSystem : IDisposable
    {
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
