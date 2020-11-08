using System;
using System.Collections.Generic;
using Automata.Engine.Collections;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public interface IMeshingStrategy
    {
        public void Mesh(Span<ushort> blocks, Span<Direction> faces, ICollection<PackedVertex> vertexes, ICollection<uint> indexes,
            IReadOnlyList<Palette<ushort>?> neighbors, int index, int localPosition, ushort blockID, bool isTransparent);
    }
}
