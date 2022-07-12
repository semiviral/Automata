using System;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class ChunkModificationsSystem : ComponentSystem
    {
        private readonly VoxelWorld _VoxelWorld;

        public ChunkModificationsSystem(VoxelWorld voxelWorld) : base(voxelWorld) => _VoxelWorld = voxelWorld;

        [HandledComponents(EnumerationStrategy.All, typeof(Chunk))]
        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            await _VoxelWorld.SynchronizeConcurrentModifications();

            foreach (Chunk chunk in entityManager.GetComponents<Chunk>())
            {
                if (chunk.State is GenerationState.AwaitingMesh or GenerationState.Finished)
                {
                    bool remeshable = true;

                    foreach (Chunk? neighbor in chunk.Neighbors)
                    {
                        if (neighbor?.State is GenerationState.GeneratingMesh)
                        {
                            remeshable = false;
                            break;
                        }
                    }

                    if (remeshable && TryProcessChunkModifications(chunk))
                    {
                        chunk.DangerousRemeshNeighborhood();
                    }
                }
            }
        }

        private static bool TryProcessChunkModifications(Chunk chunk)
        {
            bool modified = false;

            while (chunk.Modifications.TryTake(out ChunkModification? modification) && (chunk.Blocks![modification!.BlockIndex].ID != modification.BlockID))
            {
                chunk.Blocks[modification.BlockIndex] = new Block(modification.BlockID);
                modified = true;
            }

            return modified;
        }
    }
}
