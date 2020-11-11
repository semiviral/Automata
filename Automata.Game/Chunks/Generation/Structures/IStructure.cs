using System;
using System.Collections.Generic;
using Automata.Engine.Numerics;

namespace Automata.Game.Chunks.Generation.Structures
{
    public interface IStructure
    {
        public string Name { get; }
        public IEnumerable<(Vector3i, ushort)> StructureBlocks { get; }

        public bool CheckPlaceStructureAt(Random seeded, Vector3i global);
    }
}
