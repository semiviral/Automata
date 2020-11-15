using System;
using System.Collections.Generic;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public interface IMeshingStrategy
    {
        public void Mesh(Span<Block> blocks, Span<Direction> faces, ICollection<QuadVertexes<PackedVertex>> vertexes, ICollection<QuadIndexes> indexes,
            IReadOnlyList<Palette<Block>?> neighbors, int index, int localPosition, Block block, bool isTransparent);
    }
}
