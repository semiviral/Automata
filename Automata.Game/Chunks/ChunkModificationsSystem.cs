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
            foreach (Chunk chunk in entityManager.GetComponents<Chunk>()
                .Where(chunk => chunk.NeighborhoodState(GenerationState.AwaitingMesh, GenerationState.Finished)))
            {
                Debug.Assert(chunk.Blocks is not null, $"Blocks should not be null when state is {nameof(GenerationState.Finished)}.");

                bool modified = false;

                while (chunk.Modifications.TryTake(out ChunkModification? modification))
                {
                    int index = Vector3i.Project1D(modification.Local, GenerationConstants.CHUNK_SIZE);

                    if (chunk.Blocks[index].ID == modification.BlockID) continue;

                    if (!modified) modified = true;
                    chunk.Blocks[index] = new Block(modification.BlockID);
                }

                if (!modified) continue;

                chunk.State = GenerationState.AwaitingMesh;

                foreach (Chunk? neighbor in chunk.Neighbors.Where(neighbor => neighbor is not null)) neighbor!.State = GenerationState.AwaitingMesh;

                Debug.Assert(chunk.Neighbors.All(neighbor => neighbor?.State is null or GenerationState.AwaitingMesh),
                    "Neighbors should all be awaiting remesh.");
            }

            return ValueTask.CompletedTask;
        }
    }
}
