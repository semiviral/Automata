namespace Automata.Core.Components
{
    /// <summary>
    ///     Provides a derivable class for marking a component as a Tag, rather than a normal <see cref="IComponent" />
    /// </summary>
    /// <remarks>
    ///     This type of <see cref="IComponent" /> shouldn't usually contain properties or fields. It should act purely as a
    ///     hint to systems.
    /// </remarks>
    public abstract class ComponentTag : IComponent { }
}
