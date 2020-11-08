#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;
using Automata.Engine.Numerics;
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

        public const string DEFAULT_STRATEGY = "Block";

        public static readonly Dictionary<string, IMeshingStrategy> MeshingStrategies = new Dictionary<string, IMeshingStrategy>();

        public static PendingMesh<PackedVertex> GeneratePackedMesh(Palette<ushort> blocksCollection, Palette<ushort>?[] neighbors)
        {
            if ((blocksCollection.LookupTable.Count == 1) && (blocksCollection.LookupTable[0] == BlockRegistry.AirID)) return PendingMesh<PackedVertex>.Empty;

            TransparentList<PackedVertex> vertexes = new TransparentList<PackedVertex>(_DEFAULT_VERTEXES_CAPACITY);
            TransparentList<uint> indexes = new TransparentList<uint>(_DEFAULT_INDEXES_CAPACITY);
            Span<ushort> blocks = stackalloc ushort[GenerationConstants.CHUNK_SIZE_CUBED];
            Span<Direction> faces = stackalloc Direction[GenerationConstants.CHUNK_SIZE_CUBED];
            faces.Clear();

            blocksCollection.CopyTo(blocks);

            for (int index = 0, y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                ushort currentBlockID = blocks[index];

                if (currentBlockID == BlockRegistry.AirID) continue;

                int localPosition = x | (y << GenerationConstants.CHUNK_SIZE_SHIFT) | (z << (GenerationConstants.CHUNK_SIZE_SHIFT * 2));



                (blocks, faces, vertexes, indexes, neighbors, index, localPosition, currentBlockID,
                    BlockRegistry.Instance.CheckBlockHasProperty(currentBlockID, Block.Attribute.Transparent));
            }

            return vertexes.Count == 0 ? PendingMesh<PackedVertex>.Empty : new PendingMesh<PackedVertex>(vertexes.Segment, indexes.Segment);
        }
    }
}
