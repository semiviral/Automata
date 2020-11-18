
using System;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game.Blocks;



namespace Automata.Game.Chunks.Generation.Meshing
{
    public static class ChunkMesher
    {
        // these are semi-magic defaults, based on a collective average
        private const int _DEFAULT_VERTEXES_CAPACITY = 256;
        private const int _DEFAULT_INDEXES_CAPACITY = (256 * 3) / 2;

        public const string DEFAULT_STRATEGY = "Cube";

        public static readonly MeshingStrategies MeshingStrategies;

        static ChunkMesher() =>
            MeshingStrategies = new MeshingStrategies
            {
                [DEFAULT_STRATEGY] = new CubeMeshingStrategy(),
                ["X"] = new XMeshingStrategy()
            };

        [SkipLocalsInit]
        public static unsafe NonAllocatingQuadsMeshData<uint, PackedVertex> GeneratePackedMesh(Palette<Block> blocksPalette, Palette<Block>?[] neighbors)
        {
            try
            {
                if ((blocksPalette.LookupTableSize == 1) && (blocksPalette.GetLookupIndex(0).ID == BlockRegistry.AirID))
                    return NonAllocatingQuadsMeshData<uint, PackedVertex>.Empty;

                BlockRegistry blockRegistry = BlockRegistry.Instance;
                NonAllocatingList<QuadIndexes<uint>> indexes = new NonAllocatingList<QuadIndexes<uint>>(_DEFAULT_INDEXES_CAPACITY);
                NonAllocatingList<QuadVertexes<PackedVertex>> vertexes = new NonAllocatingList<QuadVertexes<PackedVertex>>(_DEFAULT_VERTEXES_CAPACITY);
                Span<Block> blocks = stackalloc Block[GenerationConstants.CHUNK_SIZE_CUBED];
                Span<Direction> faces = stackalloc Direction[GenerationConstants.CHUNK_SIZE_CUBED];
                faces.Clear();

                blocksPalette.CopyTo(blocks);

                for (int index = 0, y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
                {
                    Block block = blocks[index];

                    if ((block.ID == BlockRegistry.AirID) || (block.ID == BlockRegistry.NullID)) continue;

                    IMeshingStrategy meshingStrategy = MeshingStrategies[blockRegistry.GetBlockDefinition(block.ID).MeshingStrategyIndex];
                    int localPosition = x | (y << GenerationConstants.CHUNK_SIZE_SHIFT) | (z << (GenerationConstants.CHUNK_SIZE_SHIFT * 2));

                    meshingStrategy.Mesh(blocks, faces, indexes, vertexes, neighbors, index, localPosition, block,
                        blockRegistry.CheckBlockHasProperty(block.ID, IBlockDefinition.Attribute.Transparent));
                }

                return new NonAllocatingQuadsMeshData<uint, PackedVertex>(indexes, vertexes);
            }
            catch (Exception exception)
            {
                if (exception is IndexOutOfRangeException or InvalidOperationException && blocksPalette.Count is 0)
                    return NonAllocatingQuadsMeshData<uint, PackedVertex>.Empty;
                else throw;
            }
        }
    }
}
