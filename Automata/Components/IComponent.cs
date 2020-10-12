namespace Automata.Components
{
    public interface IComponent { }

    public interface IComponentTag : IComponent { }

    public interface IComponentChangeable : IComponent
    {
        public bool Changed { get; set; }
    }
}
