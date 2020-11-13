using System;
using System.Collections.Generic;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Structures
{
    public record TestStructure : IStructure
    {
        public string Name { get; } = "Test";
        public IEnumerable<(Vector3i, ushort)> StructureBlocks { get; } = GetStructureBlocks();

        public bool CheckPlaceStructureAt(Random seeded, Vector3i _) => seeded.Next(0, 80000) == 0;

        private static IEnumerable<(Vector3i Local, ushort BlockID)> GetStructureBlocks()
        {
            ushort sandID = BlockRegistry.Instance.GetBlockID("Core:Sand");

            for (int y = 0; y < 5; y++) yield return (new Vector3i(0, y, 0), sandID);
        }
    }
}
