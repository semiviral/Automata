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
        public IEnumerable<(Vector3i, ushort)> StructureBlocks { get; } = GetStructureBlocks();

        public bool CheckPlaceStructureAt(World world, Random seeded, Vector3i global) =>
            world is VoxelWorld voxelWorld
            && voxelWorld.TryGetBlock(global, out Block block)
            && (block.ID == _GrassID)
            && (seeded.Next(0, 8000) == 0);

        private static IEnumerable<(Vector3i Local, ushort BlockID)> GetStructureBlocks()
        {
            ushort sandID = BlockRegistry.Instance.GetBlockID("Core:Sand");

            int yTotal = 0;

            for (; yTotal < 5; yTotal++)
            {
                yield return (new Vector3i(0, yTotal, 0), sandID);
            }

            for (int y = -1; y < 2; y++)
            for (int z = -1; z < 2; z++)
            for (int x = -1; x < 2; x++)
            {
                yield return (new Vector3i(x, y + yTotal, z), sandID);
            }
        }
    }
}
