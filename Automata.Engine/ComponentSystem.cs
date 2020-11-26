using System;
using System.Threading.Tasks;

namespace Automata.Engine
{
    public abstract class ComponentSystem
    {
        protected World World { get; }

        public bool Enabled { get; protected set; }

        public ComponentSystem(World world)
        {
            World = world;
            Enabled = true;
        }

        /// <summary>
        ///     Method called when the <see cref="SystemManager" /> registers the system.
        /// </summary>
        public virtual void Registered(EntityManager entityManager) { }

        /// <summary>
        ///     Method called once per frame.
        /// </summary>
        public virtual ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta) => ValueTask.CompletedTask;

        protected TComponentSystem? GetSystem<TComponentSystem>() where TComponentSystem : ComponentSystem => World.SystemManager.GetSystem<TComponentSystem>();
    }
}
