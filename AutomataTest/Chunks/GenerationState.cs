#region

using Automata.Components;

#endregion

namespace AutomataTest.Chunks
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
