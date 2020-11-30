using System;
using System.Collections.Generic;
using Automata.Engine;
using Automata.Engine.Numerics;

namespace Automata.Game.Chunks.Generation.Structures
{
    public interface IStructure
    {
        public string Name { get; }
        public IEnumerable<(Vector3<int>, ushort)> StructureBlocks { get; }

        public bool CheckPlaceStructureAt(World world, Random seeded, Vector3<int> global);
    }
}
