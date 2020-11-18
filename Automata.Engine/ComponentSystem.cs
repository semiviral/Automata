using System;
using System.Threading.Tasks;

namespace Automata.Engine
{
    public abstract class ComponentSystem
    {
        protected World _CurrentWorld { get; private set; } = null!;
        public bool Enabled { get; protected set; }

        public ComponentSystem() => Enabled = true;

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered(EntityManager entityManager) { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        public virtual ValueTask Update(EntityManager entityManager, TimeSpan delta) => ValueTask.CompletedTask;

        protected TComponentSystem? GetSystem<TComponentSystem>() where TComponentSystem : ComponentSystem =>
            _CurrentWorld?.SystemManager.GetSystem<TComponentSystem>();

        internal void SetCurrentWorld(World currentWorld) => _CurrentWorld = currentWorld;
    }
}
