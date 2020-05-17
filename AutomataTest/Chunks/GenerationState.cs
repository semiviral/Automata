#region

using Automata;

#endregion

namespace AutomataTest.Chunks
{
    public enum GenerationState
    {
        Deactivated,
        Unbuilt,
        AwaitingBuilding,
        Unmeshed,
        AwaitingMeshing,
        Meshed
    }

    public class ChunkState : IComponent
    {
        public GenerationState Value { get; set; }
    }
}
