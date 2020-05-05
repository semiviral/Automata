namespace Automata.Core
{
    public abstract class ComponentSystem
    {
        public bool IsEnabled { get; set; }

        protected ComponentSystem(int order = SystemManager.DEFAULT_SYSTEM_ORDER)
        {
            SystemManager.RegisterSystem(this, order);
        }

        public virtual void Registered() { }

        public virtual void Enabled() { }

        public virtual void Update() { }
    }
}
