#region

using System;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game.Blocks;

#endregion


namespace Automata.Game.Chunks.Generation.Meshing
{
    public static class ChunkMesher
    {
        // these are semi-magic defaults, based on a collective average
        private const int _DEFAULT_QUADS_CAPACITY = 512;

        public const string DEFAULT_STRATEGY = "Cube";

        public static readonly MeshingStrategies MeshingStrategies;

        static ChunkMesher() =>
            MeshingStrategies = new MeshingStrategies
            {
                [DEFAULT_STRATEGY] = new CubeMeshingStrategy(),
                ["X"] = new XMeshingStrategy()
            };

        [SkipLocalsInit]
        public static unsafe NonAllocatingList<Quad<PackedVertex>> GeneratePackedMesh(Palette<Block> blocksPalette, Palette<Block>?[] neighbors)
        {
            try
            {
                if ((blocksPalette.ReadOnlyLookupTable.Count == 1) && (blocksPalette.ReadOnlyLookupTable[0].ID == BlockRegistry.AirID))
                    return NonAllocatingList<Quad<PackedVertex>>.Empty;

                BlockRegistry blockRegistry = BlockRegistry.Instance;
                NonAllocatingList<Quad<PackedVertex>> quads = new NonAllocatingList<Quad<PackedVertex>>(_DEFAULT_QUADS_CAPACITY);
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

                    meshingStrategy.Mesh(blocks, faces, quads, neighbors, index, localPosition, block,
                        blockRegistry.CheckBlockHasProperty(block.ID, BlockDefinitionDefinition.Attribute.Transparent));
                }

                return quads;
            }
            catch (Exception exception)
            {
                if (exception is IndexOutOfRangeException or InvalidOperationException && blocksPalette.Count is 0)
                    return NonAllocatingList<Quad<PackedVertex>>.Empty;
                else throw;
            }
        }
    }
}
