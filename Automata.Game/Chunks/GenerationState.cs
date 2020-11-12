namespace Automata.Game.Chunks
{
    public enum GenerationState
    {
        Deactivated,
        AwaitingTerrain,
        GeneratingTerrain,
        // AwaitingStructures,
        // GeneratingStructures,
        AwaitingMesh,
        GeneratingMesh,
        Finished
    }
}
