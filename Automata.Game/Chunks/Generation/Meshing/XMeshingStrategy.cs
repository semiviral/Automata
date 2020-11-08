using System;
using System.Collections.Generic;
using Automata.Engine.Collections;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public class XMeshingStrategy : IMeshingStrategy
    {
        public void Mesh(Span<ushort> blocks, Span<Direction> faces, ICollection<PackedVertex> vertexes, ICollection<uint> indexes,
            IReadOnlyList<Palette<ushort>?> neighbors, int index, int localPosition, ushort blockID, bool isTransparent)
        {
            int textureDepth = TextureAtlas.Instance.GetTileDepth(BlockRegistry.Instance.GetBlockName(blockID)) << (GenerationConstants.CHUNK_SIZE_SHIFT * 2);

            uint indexesStart = (uint)vertexes.Count;
            indexes.Add(indexesStart + 0u);
            indexes.Add(indexesStart + 1u);
            indexes.Add(indexesStart + 3u);
            indexes.Add(indexesStart + 1u);
            indexes.Add(indexesStart + 2u);
            indexes.Add(indexesStart + 3u);

            vertexes.Add(new PackedVertex(localPosition + 0b00_01_10_000001_000001_000001, 0b000000_000000_000001 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b00_01_10_000001_000000_000001, 0b000000_000001_000001 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b00_01_10_000000_000000_000000, 0b000000_000001_000000 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b00_01_10_000000_000001_000000, 0b000000_000000_000000 | textureDepth));

            indexesStart += 4;
            indexes.Add(indexesStart + 0u);
            indexes.Add(indexesStart + 1u);
            indexes.Add(indexesStart + 3u);
            indexes.Add(indexesStart + 1u);
            indexes.Add(indexesStart + 2u);
            indexes.Add(indexesStart + 3u);

            vertexes.Add(new PackedVertex(localPosition + 0b10_01_10_000001_000001_000000, 0b000000_000000_000001 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b10_01_10_000001_000000_000000, 0b000000_000001_000001 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b10_01_10_000000_000000_000001, 0b000000_000001_000000 | textureDepth));
            vertexes.Add(new PackedVertex(localPosition + 0b10_01_10_000000_000001_000001, 0b000000_000000_000000 | textureDepth));
        }
    }
}
