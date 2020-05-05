namespace Automata.Core
{
    public interface IComponentSystem
    {
        bool IsEnabled { get; set; }
        void Registered();
        void Enabled();
        void Update();
    }

    public abstract class ComponentSystem : IComponentSystem
    {
        public bool IsEnabled { get; set; }

        public virtual void Registered() { }

        public virtual void Enabled() { }

        public virtual void Update() { }
    }
}
