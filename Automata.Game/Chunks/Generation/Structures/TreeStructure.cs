using System;
using System.Collections.Generic;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Structures
{
    public record TestStructure : IStructure
    {
        public string Name { get; } = "Test";
        public IEnumerable<ChunkModification> StructureBlocks { get; } = GetStructureBlocks();

        public bool CheckPlaceStructureAt(Random seeded, Vector3i _) => seeded.Next(0, 8) == 0;

        private static IEnumerable<ChunkModification> GetStructureBlocks()
        {
            ushort sandID = BlockRegistry.Instance.GetBlockID("Core:Sand");

            for (int y = 0; y < 5; y++)
                yield return new ChunkModification
                {
                    Local = Vector3i.Zero.ReplaceComponent(1, y),
                    BlockID = sandID
                };
        }
    }
}
