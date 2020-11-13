namespace Automata.Game.Chunks
{
    public enum GenerationState
    {
        Inactive,
        AwaitingTerrain,
        GeneratingTerrain,
        AwaitingStructures,
        GeneratingStructures,
        AwaitingMesh,
        GeneratingMesh,
        Finished
    }
}
