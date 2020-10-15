#region

using Automata.Engine.Components;

#endregion

namespace Automata.Game.Chunks
{
    public enum GenerationState
    {
        Deactivated,
        Ungenerated,
        AwaitingBuilding,
        AwaitingMeshing,
        Finished
    }

    public class ChunkState : IComponent
    {
        public GenerationState Value { get; set; }
    }
}
