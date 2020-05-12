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

    public struct ChunkGenerationState : IComponent
    {
        public ChunkState State { get; set; }
    }
}
