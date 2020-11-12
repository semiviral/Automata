using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using Automata.Game.Chunks.Generation;

namespace Automata.Game.Chunks
{
    public class ChunkModificationsSystem : ComponentSystem
    {
        [HandledComponents(DistinctionStrategy.All, typeof(Chunk))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool CanCommitModifications(Chunk chunk) => chunk.State is GenerationState.Finished or GenerationState.AwaitingMesh
                                                               && chunk.IsStateLockstep(true);

            foreach (Chunk chunk in entityManager.GetComponents<Chunk>().Where(CanCommitModifications))
            {
                Debug.Assert(chunk.Blocks is not null, $"Blocks should not be null when state is {nameof(GenerationState.Finished)}.");

                while (chunk.Modifications.TryTake(out ChunkModification? modification))
                {
                    int index = Vector3i.Project1D(modification.Local, GenerationConstants.CHUNK_SIZE);

                    if (chunk.Blocks[index].ID != modification.BlockID)
                    {
                        // set state to prohibit lockstep mesh gen
                        if (chunk.State is not GenerationState.GeneratingStructures) chunk.State = GenerationState.GeneratingStructures;
                        chunk.Blocks[index] = new Block(modification.BlockID);
                    }
                }

                if (chunk.State is GenerationState.AwaitingMesh or not GenerationState.GeneratingStructures) continue;

                chunk.State = GenerationState.AwaitingMesh;
                chunk.RemeshNeighbors();
            }

            return ValueTask.CompletedTask;
        }
    }
}
