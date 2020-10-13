#region

using Automata.Components;

#endregion

namespace AutomataTest.Chunks
{
    public enum GenerationState
    {
        Deactivated,
        Ungenerated,
        AwaitingGeneration,
        Unmeshed,
        AwaitingMeshing,
        Meshed
    }

    public class ChunkState : IComponent
    {
        public GenerationState Value { get; set; }
    }
}
