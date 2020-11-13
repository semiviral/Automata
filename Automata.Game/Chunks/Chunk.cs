using System.Collections.Generic;
using System.Linq;
using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        public GenerationState State { get; set; }
        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public IEnumerable<Palette<Block>?> NeighborBlocks => Neighbors.Select(chunk => chunk?.Blocks);
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, false);

        public bool NeighborhoodState(GenerationState state1)
        {
            if (State != state1) return false;

            foreach (Chunk? neighbor in Neighbors.Where(neighbor => neighbor is not null))
                if (neighbor!.State != state1)
                    return false;

            return true;
        }

        public bool NeighborhoodState(GenerationState state1, GenerationState state2)
        {
            if ((State != state1) && (State != state2)) return false;

            foreach (Chunk? neighbor in Neighbors.Where(neighbor => neighbor is not null))
                if ((neighbor!.State != state1) && (neighbor!.State != state2))
                    return false;

            return true;
        }

        public bool NeighborState(GenerationState state1, ComparisonMode comparisonMode) =>
            comparisonMode switch
            {
                ComparisonMode.EqualOrGreaterThan => Neighbors.All(neighbor => neighbor is null || (neighbor.State >= state1)),
                ComparisonMode.EqualOrLessThan => Neighbors.All(neighbor => neighbor is null || (neighbor.State <= state1)),
                ComparisonMode.Equal => Neighbors.All(neighbor => neighbor is null || (neighbor.State == state1)),
                _ => false
            };

        public void RemeshNeighborhood(bool remesh)
        {
            if (!remesh) return;

            State = GenerationState.AwaitingMesh;

            foreach (Chunk? neighbor in Neighbors.Where(neighbor => neighbor is not null)) neighbor!.State = GenerationState.AwaitingMesh;
        }

        //public override string ToString() => $"chunk({State}, {InsertionSafeState}, {IsStateLockstep(ComparisonMode.Equal)}, {Neighbors.Count(neighbor => neighbor is null)}";
    }
}
