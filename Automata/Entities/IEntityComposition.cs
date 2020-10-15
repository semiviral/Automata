#region

using Automata.Systems;

#endregion

namespace Automata.Entities
{
    public interface IEntityComposition
    {
        public ComponentTypes ComposedTypes { get; }
    }
}
