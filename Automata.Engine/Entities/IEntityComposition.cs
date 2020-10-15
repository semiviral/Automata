#region

using Automata.Engine.Systems;

#endregion

namespace Automata.Engine.Entities
{
    public interface IEntityComposition
    {
        public ComponentTypes ComposedTypes { get; }
    }
}
