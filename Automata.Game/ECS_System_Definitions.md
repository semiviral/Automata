## ChunkModificationSystem `[Automata.Game.Chunks]`
- Neighborhood Dependent: Yes
    - Only Runs When:
        - `GenerationState.AwaitingMesh`
        - `GenerationState.Finished`


- Modifies Chunk State: Yes
    - Modifies Neighbors: Yes
        - Modifies When:
            - `Chunk.Modifications.TryTake()` is `true`
            - and
            - `Chunk.Blocks[Vector3i.Project1D(Modification.Local, CHUNK_SIZE)]` is not `Modification.BlockID`
                - *Explanation: when the existing block does not match the modification block.*
        - Modifies With:
            - Includes Neighbors: Yes
            - `Chunk.State` becomes `GenerationState.AwaitingMesh`