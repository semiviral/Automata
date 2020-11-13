using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Automata.Engine.Entities;
using Automata.Engine.Systems;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class ChunkModificationsSystem : ComponentSystem
    {
        [HandledComponents(DistinctionStrategy.All, typeof(Chunk))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (Chunk chunk in entityManager.GetComponents<Chunk>())
            {
                if (!chunk.NeighborhoodState(GenerationState.AwaitingMesh, GenerationState.Finished) || !TryProcessChunkModifications(chunk)) continue;

                chunk.RemeshNeighborhood(true);
                Debug.Assert(chunk.NeighborhoodState(GenerationState.AwaitingMesh), "Neighbors should all be awaiting remesh.");
            }

            return ValueTask.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryProcessChunkModifications(Chunk chunk)
        {
            Debug.Assert(chunk.Blocks is not null);

            bool modified = false;

            while (chunk.Modifications.TryTake(out ChunkModification? modification) && (chunk.Blocks[modification.BlockIndex].ID != modification.BlockID))
            {
                chunk.Blocks[modification.BlockIndex] = new Block(modification.BlockID);
                modified = true;
            }

            return modified;
        }
    }
}
