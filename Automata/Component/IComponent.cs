namespace Automata
{
    public interface IComponent { }

    public interface IComponentTag : IComponent { }

    public interface IComponentChangeable : IComponent
    {
        public bool Changed { get; set; }
    }
}
