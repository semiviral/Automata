using Automata.Core.Components;

namespace AutomataTest.Chunks
{
    public enum ChunkState
    {
        Unbuilt,
        AwaitingBuilding,
        Unmeshed,
        AwaitingMeshing,
        Meshed
    }

    public class GenerationState : IComponent
    {
        public ChunkState State { get; set; }
    }
}
