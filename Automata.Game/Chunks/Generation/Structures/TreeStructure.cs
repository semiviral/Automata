using System;
using System.Collections.Generic;
using Automata.Engine;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Structures
{
    public record TreeStructure : IStructure
    {
        private static readonly ushort _GrassID = BlockRegistry.Instance.GetBlockID("Core:Grass");

        public string Name { get; } = "Test";
        public IEnumerable<(Vector3<int>, ushort)> StructureBlocks { get; } = GetStructureBlocks();

        public bool CheckPlaceStructureAt(World world, Random seeded, Vector3<int> global) =>
            world is VoxelWorld voxel_world
            && voxel_world.TryGetBlock(global, out Block block)
            && (block.ID == _GrassID)
            && (seeded.Next(0, 8000) == 0);

        private static IEnumerable<(Vector3<int> Local, ushort BlockID)> GetStructureBlocks()
        {
            ushort sand_id = BlockRegistry.Instance.GetBlockID("Core:Sand");

            int y_total = 0;

            for (; y_total < 5; y_total++)
            {
                yield return (new Vector3<int>(0, y_total, 0), sand_id);
            }

            for (int y = -1; y < 2; y++)
            for (int z = -1; z < 2; z++)
            for (int x = -1; x < 2; x++)
            {
                yield return (new Vector3<int>(x, y + y_total, z), sand_id);
            }
        }
    }
}
