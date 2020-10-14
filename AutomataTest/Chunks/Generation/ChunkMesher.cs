using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Documents;
using Automata;
using Automata.Collections;
using Automata.Numerics;
using Automata.Rendering.Meshes;
using AutomataTest.Blocks;

namespace AutomataTest.Chunks.Generation
{
    public static class ChunkMesher
    {

        private static readonly int[][] _VertexesByIteration =
        {
            // 3   0
            //
            // 2   1

            // z y x
            new[]
            {
                // East      z      y      x
                0b01_01_10_000001_000001_000001,
                0b01_01_10_000001_000000_000001,
                0b01_01_10_000000_000000_000001,
                0b01_01_10_000000_000001_000001,
            },
            new[]
            {
                // Up        z      y      x
                0b01_10_01_000001_000001_000000,
                0b01_10_01_000001_000001_000001,
                0b01_10_01_000000_000001_000001,
                0b01_10_01_000000_000001_000000,
            },
            new[]
            {
                // North     z      y      x
                0b10_01_01_000001_000001_000000,
                0b10_01_01_000001_000000_000000,
                0b10_01_01_000001_000000_000001,
                0b10_01_01_000001_000001_000001,
            },
            new[]
            {
                // West      z      y      x
                0b01_01_00_000000_000001_000000,
                0b01_01_00_000000_000000_000000,
                0b01_01_00_000001_000000_000000,
                0b01_01_00_000001_000001_000000,
            },
            new[]
            {
                // Down      z      y      x
                0b01_00_01_000001_000000_000001,
                0b01_00_01_000001_000000_000000,
                0b01_00_01_000000_000000_000000,
                0b01_00_01_000000_000000_000001,
            },
            new[]
            {
                // South     z      y      x
                0b00_01_01_000000_000001_000001,
                0b00_01_01_000000_000000_000001,
                0b00_01_01_000000_000000_000000,
                0b00_01_01_000000_000001_000000,
            },
        };

        private static readonly int[] _IndexStepByNormalIndex =
        {
            1,
            GenerationConstants.CHUNK_SIZE_SQUARED,
            GenerationConstants.CHUNK_SIZE,
            -1,
            -GenerationConstants.CHUNK_SIZE_SQUARED,
            -GenerationConstants.CHUNK_SIZE,
        };

        public static PendingMesh<int> GenerateMesh(Span<ushort> blocks, INodeCollection<ushort>[] neighbors)
        {
            Span<Direction> faces = stackalloc Direction[blocks.Length];
            List<int> vertexes = new List<int>();
            List<uint> indexes = new List<uint>();

            int index = 0;

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, index++)
            {
                ushort currentBlockId = blocks[index];

                if (currentBlockId == BlockRegistry.AirID)
                {
                    continue;
                }

                int localPosition = x | (y << GenerationConstants.CHUNK_SIZE_BIT_SHIFT) | (z << (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * 2));

                TraverseIndex(blocks, faces, vertexes, indexes, neighbors, index, localPosition, currentBlockId,
                    BlockRegistry.Instance.CheckBlockHasProperty(currentBlockId, BlockDefinition.Property.Transparent));
            }

            return new PendingMesh<int>(vertexes.ToArray(), indexes.ToArray());
        }

        private static void TraverseIndex(Span<ushort> blocks, Span<Direction> faces, ICollection<int> vertexes, ICollection<uint> indexes,
            IReadOnlyList<INodeCollection<ushort>> neighbors, int index, int localPosition, ushort currentBlockId, bool isCurrentBlockTransparent)
        {
            // iterate once over all 6 faces of given cubic space
            for (int normalIndex = 0; normalIndex < 6; normalIndex++)
            {
                // face direction always exists on a single bit, so shift 1 by the current normalIndex (0-5)
                Direction faceDirection = (Direction)(1 << normalIndex);

                // check if current index has face already
                if (faces[index].HasDirection(faceDirection))
                {
                    continue;
                }

                // indicates whether the current face checking direction is negative or positive
                bool isNegativeFace = (normalIndex - 3) >= 0;
                // normalIndex constrained to represent the 3 axes
                int iModulo3 = normalIndex % 3;
                int iModulo3Shift = GenerationConstants.CHUNK_SIZE_BIT_SHIFT * iModulo3;
                // axis value of the current face check direction
                // example: for iteration normalIndex == 0—which is positive X—it'd be equal to localPosition.x
                int faceCheckAxisValue = (localPosition >> iModulo3Shift) & GenerationConstants.CHUNK_SIZE_BIT_MASK;
                // indicates whether or not the face check is within the current chunk bounds
                bool isFaceCheckOutOfBounds = (!isNegativeFace && (faceCheckAxisValue == (GenerationConstants.CHUNK_SIZE - 1)))
                                              || (isNegativeFace && (faceCheckAxisValue == 0));
                // total number of successful traversals
                // remark: this is outside the for loop so that the if statement after can determine if any traversals have happened
                int traversals = 0;

                for (int perpendicularNormalIndex = 1; perpendicularNormalIndex < 3; perpendicularNormalIndex++)
                {
                    // the index of the int3 traversalNormal to traverse on
                    int traversalNormalIndex = (iModulo3 + perpendicularNormalIndex) % 3;
                    int traversalNormalShift = GenerationConstants.CHUNK_SIZE_BIT_SHIFT * traversalNormalIndex;

                    // current value of the local position by traversal direction
                    int traversalNormalAxisValue = (localPosition >> traversalNormalShift) & GenerationConstants.CHUNK_SIZE_BIT_MASK;
                    // amount by integer to add to current index to get 3D->1D position of traversal position
                    int traversalIndexStep = _IndexStepByNormalIndex[traversalNormalIndex];
                    // current traversal index, which is increased by traversalIndexStep every iteration the for loop below
                    int traversalIndex = index + (traversals * traversalIndexStep);
                    // local start axis position + traversals
                    int totalTraversalLength = traversalNormalAxisValue + traversals;

                    for (;
                        (totalTraversalLength < GenerationConstants.CHUNK_SIZE)
                        && !faces[traversalIndex].HasDirection(faceDirection)
                        && (blocks[traversalIndex] == currentBlockId);
                        totalTraversalLength++,
                        traversals++, // increment traversals
                        traversalIndex += traversalIndexStep) // increment traversal index by index step to adjust local working position
                    {
                        // check if current facing block axis value is within the local chunk
                        if (!isFaceCheckOutOfBounds)
                        {
                            // amount by integer to add to current traversal index to get 3D->1D position of facing block
                            int facedBlockIndex = traversalIndex + _IndexStepByNormalIndex[normalIndex];
                            // if so, index into block ids and set facingBlockId
                            ushort facedBlockId = blocks[facedBlockIndex];

                            // if transparent, traverse so long as facing block is not the same block id
                            // if opaque, traverse so long as facing block is transparent
                            if (isCurrentBlockTransparent)
                            {
                                if (currentBlockId != facedBlockId)
                                {
                                    break;
                                }
                            }
                            else if (!BlockRegistry.Instance.CheckBlockHasProperty(facedBlockId, BlockDefinition.Property.Transparent))
                            {
                                if (!isNegativeFace)
                                {
                                    // we've culled this face, and faced block is opaque as well, so cull it's face inverse to the current.
                                    Direction inverseFaceDirection = (Direction)(1 << ((normalIndex + 3) % 6));
                                    faces[facedBlockIndex] = faces[facedBlockIndex].WithDirection(inverseFaceDirection);
                                }

                                break;
                            }
                        }
                        else
                        {
                            // this block of code translates the integer local position to the local position of the neighbor at [normalIndex]
                            int sign = isNegativeFace ? -1 : 1;
                            int iModuloComponentMask = GenerationConstants.CHUNK_SIZE_BIT_MASK << iModulo3Shift;
                            int translatedLocalPosition = localPosition + (traversals << traversalNormalShift);
                            int finalLocalPosition = (~iModuloComponentMask & translatedLocalPosition)
                                                     | (AutomataMath.Wrap(((translatedLocalPosition & iModuloComponentMask) >> iModulo3Shift) + sign,
                                                            GenerationConstants.CHUNK_SIZE, 0, GenerationConstants.CHUNK_SIZE - 1)
                                                        << iModulo3Shift);

                            // index into neighbor blocks collections, call .GetPoint() with adjusted local position
                            // remark: if there's no neighbor at the index given, then no chunk exists there (for instance,
                            //     chunks at the edge of render distance). In this case, return NullID so no face is rendered on edges.
                            ushort facedBlockId = neighbors[normalIndex]?.GetPoint(DecompressVertex(finalLocalPosition))
                                                  ?? BlockRegistry.NullID;

                            if (isCurrentBlockTransparent)
                            {
                                if (currentBlockId != facedBlockId)
                                {
                                    break;
                                }
                            }
                            else if (!BlockRegistry.Instance.CheckBlockHasProperty(facedBlockId, BlockDefinition.Property.Transparent))
                            {
                                break;
                            }
                        }

                        faces[traversalIndex] = faces[traversalIndex].WithDirection(faceDirection);
                    }

                    // if it's the first traversal and we've only made a 1x1x1 face, continue to test next axis
                    if ((traversals == 1) && (perpendicularNormalIndex == 1))
                    {
                        continue;
                    }

                    if (traversals == 0 /*|| !BlockRegistry.Instance.GetUVs(currentBlockId, faceDirection, out ushort textureId)*/)
                    {
                        break;
                    }

                    // int uvShift = (iModulo3 + traversalNormalIndex) % 2;
                    // int compressedUv = (textureId << (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * 2))
                    //                    | (1 << (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * ((uvShift + 1) % 2)))
                    //                    | (traversals << (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * uvShift));

                    uint verticesCount = (uint)vertexes.Count;

                    indexes.Add(0u + verticesCount);
                    indexes.Add(1u + verticesCount);
                    indexes.Add(3u + verticesCount);
                    indexes.Add(1u + verticesCount);
                    indexes.Add(2u + verticesCount);
                    indexes.Add(3u + verticesCount);

                    int traversalShiftedMask = GenerationConstants.CHUNK_SIZE_BIT_MASK << traversalNormalShift;
                    int unaryTraversalShiftedMask = ~traversalShiftedMask;

                    int[] compressedVertices = _VertexesByIteration[normalIndex];

                    vertexes.Add(localPosition
                                  + ((unaryTraversalShiftedMask & compressedVertices[0])
                                     | ((((compressedVertices[0] >> traversalNormalShift) * traversals) << traversalNormalShift)
                                        & traversalShiftedMask)));
                    //_MeshData.AddVertex(compressedUv & (int.MaxValue << (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * 2)));


                    vertexes.Add(localPosition
                                 + ((unaryTraversalShiftedMask & compressedVertices[1])
                                    | ((((compressedVertices[1] >> traversalNormalShift) * traversals) << traversalNormalShift)
                                       & traversalShiftedMask)));
                    //_MeshData.AddVertex(compressedUv & (int.MaxValue << GenerationConstants.CHUNK_SIZE_BIT_SHIFT));


                    vertexes.Add(localPosition
                                 + ((unaryTraversalShiftedMask & compressedVertices[2])
                                    | ((((compressedVertices[2] >> traversalNormalShift) * traversals) << traversalNormalShift)
                                       & traversalShiftedMask)));
                    //_MeshData.AddVertex(compressedUv & ~(GenerationConstants.CHUNK_SIZE_BIT_MASK << GenerationConstants.CHUNK_SIZE_BIT_SHIFT));


                    vertexes.Add(localPosition
                                 + ((unaryTraversalShiftedMask & compressedVertices[3])
                                    | ((((compressedVertices[3] >> traversalNormalShift) * traversals) << traversalNormalShift)
                                       & traversalShiftedMask)));
                    //_MeshData.AddVertex(compressedUv & int.MaxValue);

                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3i DecompressVertex(int vertex) =>
            new Vector3i(vertex & GenerationConstants.CHUNK_SIZE_BIT_MASK,
                (vertex >> GenerationConstants.CHUNK_SIZE_BIT_SHIFT) & GenerationConstants.CHUNK_SIZE_BIT_MASK,
                (vertex >> (GenerationConstants.CHUNK_SIZE_BIT_SHIFT * 2)) & GenerationConstants.CHUNK_SIZE_BIT_MASK);
    }
}
