#region

using System;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.Meshes;
using Automata.Game.Blocks;

#endregion


namespace Automata.Game.Chunks.Generation.Meshing
{
    public static class ChunkMesher
    {
        // these are semi-magic defaults, based on a collective average
        private const int _DEFAULT_VERTEXES_CAPACITY = 2048;
        private const int _DEFAULT_INDEXES_CAPACITY = 3072;

        public const string DEFAULT_STRATEGY = "Cube";

        public static readonly MeshingStrategies MeshingStrategies;

        static ChunkMesher() =>
            MeshingStrategies = new MeshingStrategies
            {
                [DEFAULT_STRATEGY] = new CubeMeshingStrategy(),
                ["X"] = new XMeshingStrategy()
            };

        public static PendingMesh<PackedVertex> GeneratePackedMesh(Palette<Block> blocksPalette, Palette<Block>?[] neighbors)
        {
            if ((blocksPalette.ReadOnlyLookupTable.Count == 1) && (blocksPalette.ReadOnlyLookupTable[0].ID == BlockRegistry.AirID))
                return PendingMesh<PackedVertex>.Empty;

            BlockRegistry blockRegistry = BlockRegistry.Instance;
            TransparentList<PackedVertex> vertexes = new TransparentList<PackedVertex>(_DEFAULT_VERTEXES_CAPACITY, true);
            TransparentList<uint> indexes = new TransparentList<uint>(_DEFAULT_INDEXES_CAPACITY, true);
            Span<Block> blocks = stackalloc Block[GenerationConstants.CHUNK_SIZE_CUBED];
            Span<Direction> faces = stackalloc Direction[GenerationConstants.CHUNK_SIZE_CUBED];
            faces.Clear();

            blocksPalette.CopyTo(blocks);

            for (int index = 0, y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                Block block = blocks[index];

                if (block.ID == BlockRegistry.AirID) continue;

                IMeshingStrategy meshingStrategy = MeshingStrategies[blockRegistry.GetBlockDefinition(block.ID).MeshingStrategyIndex];
                int localPosition = x | (y << GenerationConstants.CHUNK_SIZE_SHIFT) | (z << (GenerationConstants.CHUNK_SIZE_SHIFT * 2));

                meshingStrategy.Mesh(blocks, faces, vertexes, indexes, neighbors, index, localPosition, block,
                    blockRegistry.CheckBlockHasProperty(block.ID, BlockDefinitionDefinition.Attribute.Transparent));
            }

            return vertexes.Count == 0 ? PendingMesh<PackedVertex>.Empty : new PendingMesh<PackedVertex>(vertexes.Segment, indexes.Segment);
        }
    }
}
