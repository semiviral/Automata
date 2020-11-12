using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; } = GenerationState.AwaitingTerrain;
        public bool InsertionSafeState => State is GenerationState.Finished or GenerationState.AwaitingMesh;
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public IEnumerable<Palette<Block>?> NeighborBlocks => Neighbors.Select(chunk => chunk?.Blocks);
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, false);

        public bool IsStateLockstep(ComparisonMode comparisonMode) => Neighbors.All(chunk => chunk is null
                                                                                             || comparisonMode switch
                                                                                             {
                                                                                                 ComparisonMode.EqualOrGreaterThan => chunk.State >= State,
                                                                                                 ComparisonMode.EqualOrLessThan => chunk.State <= State,
                                                                                                 ComparisonMode.Equal => chunk.State == State,
                                                                                                 _ => throw new ArgumentOutOfRangeException(
                                                                                                     nameof(comparisonMode))
                                                                                             });

        public void RemeshNeighbors()
        {
            foreach (Chunk? chunk in Neighbors)
                if (chunk?.InsertionSafeState is true)
                    chunk.State = GenerationState.AwaitingMesh;
        }
    }
}
