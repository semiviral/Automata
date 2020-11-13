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
    
## ChunkGenerationSystem `[Automata.Game.Chunks.Generation]`
- Neighborhood Dependent: Maybe
    - Only Runs When
        - `Chunk.State` is `GenerationState.AwaitingTerrain` or `GenerationState.AwaitingStructures`
        - `Chunk.State` is `GenerationState.AwaitingMesh`
        - and
        - `Chunk.Neighbors` all have state `GenerationState.AwaitingMesh` or `GenerationState.Finished`


- Modifies Chunk State: Yes
  
      |            |        Starts        |      Completes     |
      |-----------:|:--------------------:|:------------------:|
      |    Terrain |   GeneratingTerrain  | AwaitingStructures |
      | Structures | GeneratingStructures |    AwaitingMesh    |
      |       Mesh |    GeneratingMesh    |      Finished      |