#region

using System;
using Automata.Engine.Entities;
using Automata.Engine.Worlds;

#endregion


namespace Automata.Engine.Systems
{
    public abstract class ComponentSystem : IDisposable
    {
        protected World _CurrentWorld { get; private set; }
        public bool Enabled { get; protected set; }

        public ComponentSystem() => Enabled = true;

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered() { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        public virtual void Update(EntityManager entityManager, TimeSpan delta) { }

        internal void SetCurrentWorld(World currentWorld) => _CurrentWorld = currentWorld;


        #region IDisposable

        private bool _Disposed;

        protected virtual void DisposeInternal() { }

        public void Dispose()
        {
            if (_Disposed) return;

            DisposeInternal();
            GC.SuppressFinalize(this);
            _Disposed = true;
        }

        #endregion
    }
}
