namespace Automata.Core
{
    public abstract class ComponentSystem
    {
        protected ComponentSystem(int order = SystemManager.DEFAULT_SYSTEM_ORDER)
        {
            SystemManager.RegisterSystem(this, order);
        }

        public virtual void Registered() { }

        public virtual void Enable() { }

        public virtual void Update() { }
    }
}
