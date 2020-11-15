#region

using System;
using System.Threading.Tasks;
using Automata.Engine.Entities;

#endregion


namespace Automata.Engine.Systems
{
    public abstract class ComponentSystem : IDisposable
    {
        protected World? _CurrentWorld { get; private set; }
        public bool Enabled { get; protected set; }

        public ComponentSystem() => Enabled = true;

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered(EntityManager entityManager) { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        public abstract ValueTask Update(EntityManager entityManager, TimeSpan delta);

        protected TComponentSystem? GetSystem<TComponentSystem>() where TComponentSystem : ComponentSystem =>
            _CurrentWorld?.SystemManager.GetSystem<TComponentSystem>();

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
