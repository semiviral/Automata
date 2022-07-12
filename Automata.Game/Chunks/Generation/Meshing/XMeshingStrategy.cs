using System;
using System.Collections.Generic;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public class XMeshingStrategy : IMeshingStrategy
    {
        public void Mesh(Span<Block> blocks, Span<Direction> faces, ICollection<QuadIndexes<uint>> indexes, ICollection<QuadVertexes<PackedVertex>> vertexes,
            IReadOnlyList<Palette<Block>?> neighbors, int index, int localPosition, Block block, bool isTransparent)
        {
            int texture_depth = TextureAtlas.Instance.GetTileDepth(BlockRegistry.Instance.GetBlockName(block.ID)) << (GenerationConstants.CHUNK_SIZE_SHIFT * 2);

            uint indexes_start = (uint)vertexes.Count * 4u;

            indexes.Add(new QuadIndexes<uint>(
                indexes_start + 0u,
                indexes_start + 1u,
                indexes_start + 3u,
                indexes_start + 1u,
                indexes_start + 2u,
                indexes_start + 3u
            ));

            vertexes.Add(new QuadVertexes<PackedVertex>(
                new PackedVertex(localPosition + 0b00_01_10_000001_000001_000001, 0b000000_000000_000001 | texture_depth),
                new PackedVertex(localPosition + 0b00_01_10_000001_000000_000001, 0b000000_000001_000001 | texture_depth),
                new PackedVertex(localPosition + 0b00_01_10_000000_000000_000000, 0b000000_000001_000000 | texture_depth),
                new PackedVertex(localPosition + 0b00_01_10_000000_000001_000000, 0b000000_000000_000000 | texture_depth)
            ));

            indexes_start += 4;

            indexes.Add(new QuadIndexes<uint>(
                indexes_start + 0u,
                indexes_start + 1u,
                indexes_start + 3u,
                indexes_start + 1u,
                indexes_start + 2u,
                indexes_start + 3u
            ));

            vertexes.Add(new QuadVertexes<PackedVertex>(
                new PackedVertex(localPosition + 0b10_01_10_000001_000001_000000, 0b000000_000000_000001 | texture_depth),
                new PackedVertex(localPosition + 0b10_01_10_000001_000000_000000, 0b000000_000001_000001 | texture_depth),
                new PackedVertex(localPosition + 0b10_01_10_000000_000000_000001, 0b000000_000001_000000 | texture_depth),
                new PackedVertex(localPosition + 0b10_01_10_000000_000001_000001, 0b000000_000000_000000 | texture_depth)
            ));
        }
    }
}
