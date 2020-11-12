using System;
using System.Diagnostics;
using System.Linq;
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
            foreach (Chunk chunk in entityManager.GetComponents<Chunk>().Where(chunk => chunk.ThreadSafeState && chunk.IsStateLockstep(true)))
            {
                Debug.Assert(chunk.Blocks is not null, $"Blocks should not be null when state is {nameof(GenerationState.Finished)}.");

                bool modified = false;

                while (chunk.Modifications.TryTake(out ChunkModification? modification))
                {
                    int index = Vector3i.Project1D(modification.Local, GenerationConstants.CHUNK_SIZE);

                    if (chunk.Blocks[index].ID == modification.BlockID) continue;

                    // set state to prohibit lockstep mesh gen
                    if (chunk.State is not GenerationState.AwaitingMesh)
                    {
                        chunk.State = GenerationState.AwaitingMesh;
                        modified = true;
                    }

                    chunk.Blocks[index] = new Block(modification.BlockID);
                }

                if (modified) chunk.RemeshNeighbors();
            }

            return ValueTask.CompletedTask;
        }
    }
}
