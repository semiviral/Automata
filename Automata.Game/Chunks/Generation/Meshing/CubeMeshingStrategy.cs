using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata.Engine.Collections;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public struct CubeMeshingStrategy : IMeshingStrategy
    {
        private static readonly int[][] _PackedVertexesByIteration =
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
                0b01_01_10_000000_000001_000001
            },
            new[]
            {
                // Up        z      y      x
                0b01_10_01_000001_000001_000000,
                0b01_10_01_000001_000001_000001,
                0b01_10_01_000000_000001_000001,
                0b01_10_01_000000_000001_000000
            },
            new[]
            {
                // North     z      y      x
                0b10_01_01_000001_000001_000000,
                0b10_01_01_000001_000000_000000,
                0b10_01_01_000001_000000_000001,
                0b10_01_01_000001_000001_000001
            },
            new[]
            {
                // West      z      y      x
                0b01_01_00_000000_000001_000000,
                0b01_01_00_000000_000000_000000,
                0b01_01_00_000001_000000_000000,
                0b01_01_00_000001_000001_000000
            },
            new[]
            {
                // Down      z      y      x
                0b01_00_01_000001_000000_000001,
                0b01_00_01_000001_000000_000000,
                0b01_00_01_000000_000000_000000,
                0b01_00_01_000000_000000_000001
            },
            new[]
            {
                // South     z      y      x
                0b00_01_01_000000_000001_000001,
                0b00_01_01_000000_000000_000001,
                0b00_01_01_000000_000000_000000,
                0b00_01_01_000000_000001_000000
            }
        };

        private static readonly int[] _IndexStepByNormalIndex =
        {
            1,
            GenerationConstants.CHUNK_SIZE_SQUARED,
            GenerationConstants.CHUNK_SIZE,
            -1,
            -GenerationConstants.CHUNK_SIZE_SQUARED,
            -GenerationConstants.CHUNK_SIZE
        };

        public void Mesh(Span<Block> blocks, Span<Direction> faces, ICollection<QuadIndexes<uint>> indexes, ICollection<QuadVertexes<PackedVertex>> vertexes,
            IReadOnlyList<Palette<Block>?> neighbors, int index, int localPosition, Block block, bool isTransparent)
        {
            // iterate once over all 6 faces of given cubic space
            for (int normal_index = 0; normal_index < 6; normal_index++)
            {
                // face direction always exists on a single bit, so shift 1 by the current normalIndex (0-5)
                Direction face_direction = (Direction)(1 << normal_index);

                // check if current index has face already
                if (faces[index].HasDirection(face_direction))
                {
                    continue;
                }

                // indicates whether the current face checking direction is negative or positive
                bool is_negative_normal = (normal_index - 3) >= 0;

                // normalIndex constrained to represent the 3 axes
                int component_index = normal_index % 3;
                int component_shift = GenerationConstants.CHUNK_SIZE_SHIFT * component_index;

                // axis value of the current face check direction
                int faced_axis_value = (localPosition >> component_shift) & GenerationConstants.CHUNK_SIZE_MASK;

                // indicates whether or not the face check is within the current chunk bounds
                bool facing_neighbor = (!is_negative_normal && (faced_axis_value == (GenerationConstants.CHUNK_SIZE - 1)))
                                      || (is_negative_normal && (faced_axis_value == 0));

                // total number of successful traversals
                int traversals = 0;

                for (int perpendicular_normal_index = 1; perpendicular_normal_index < 3; perpendicular_normal_index++)
                {
                    // the index of the int3 traversalNormal to traverse on
                    int traversal_normal_index = (component_index + perpendicular_normal_index) % 3;
                    int traversal_normal_shift = GenerationConstants.CHUNK_SIZE_SHIFT * traversal_normal_index;

                    // current value of the local position by traversal direction
                    int traversal_normal_axis_value = (localPosition >> traversal_normal_shift) & GenerationConstants.CHUNK_SIZE_MASK;

                    // amount by integer to add to current index to get 3D->1D position of traversal position
                    int traversal_index_step = _IndexStepByNormalIndex[traversal_normal_index];

                    // current traversal index, which is increased by traversalIndexStep every iteration the for loop below
                    int traversal_index = index + (traversals * traversal_index_step);

                    // local start axis position + traversals
                    int total_traversal_length = traversal_normal_axis_value + traversals;

                    for (;
                        (total_traversal_length < GenerationConstants.CHUNK_SIZE)
                        && !faces[traversal_index].HasDirection(face_direction)
                        && (blocks[traversal_index].ID == block.ID);
                        traversal_index += traversal_index_step, // increment traversal index
                        total_traversal_length++, // increment total traversals
                        traversals++) // increment traversals
                    {
                        // check if current facing block axis value is within the local chunk
                        if (facing_neighbor)
                        {
                            // this block of code translates the integer local position to the local position of the neighbor at [normalIndex]
                            int sign = is_negative_normal ? -1 : 1;
                            int component_mask = GenerationConstants.CHUNK_SIZE_MASK << component_shift;
                            int traversal_local_position = localPosition + (traversals << traversal_normal_shift);

                            int neighbor_local_position = (~component_mask & traversal_local_position)
                                                        | (Wrap(((traversal_local_position & component_mask) >> component_shift) + sign,
                                                               GenerationConstants.CHUNK_SIZE, 0, GenerationConstants.CHUNK_SIZE - 1)
                                                           << component_shift);

                            // index into neighbor blocks collections, call .GetPoint() with adjusted local position
                            // remark: if there's no neighbor at the index given, then no chunk exists there (for instance,
                            //     chunks at the edge of render distance). In this case, return NullID so no face is rendered on edges.
                            int faced_block_index = Vector.Project1D(
                                neighbor_local_position & GenerationConstants.CHUNK_SIZE_MASK,
                                (neighbor_local_position >> (GenerationConstants.CHUNK_SIZE_SHIFT * 1)) & GenerationConstants.CHUNK_SIZE_MASK,
                                (neighbor_local_position >> (GenerationConstants.CHUNK_SIZE_SHIFT * 2)) & GenerationConstants.CHUNK_SIZE_MASK,
                                GenerationConstants.CHUNK_SIZE);

                            ushort faced_block_id = neighbors[normal_index]?[faced_block_index].ID ?? BlockRegistry.NullID;

                            if (isTransparent)
                            {
                                if (block.ID != faced_block_id)
                                {
                                    break;
                                }
                            }
                            else if (!BlockRegistry.Instance.CheckBlockHasProperty(faced_block_id, IBlockDefinition.Attribute.Transparent))
                            {
                                break;
                            }
                        }
                        else
                        {
                            // amount by integer to add to current traversal index to get 3D->1D position of facing block
                            int faced_block_index = traversal_index + _IndexStepByNormalIndex[normal_index];

                            // if so, index into block ids and set facingBlockId
                            ushort faced_block_id = blocks[faced_block_index].ID;

                            // if transparent, traverse so long as facing block is not the same block id
                            // if opaque, traverse so long as facing block is transparent
                            if (isTransparent)
                            {
                                if (block.ID != faced_block_id)
                                {
                                    break;
                                }
                            }
                            else if (!BlockRegistry.Instance.CheckBlockHasProperty(faced_block_id, IBlockDefinition.Attribute.Transparent))
                            {
                                if (!is_negative_normal)

                                    // we've culled the current face, and faced block is opaque as well, so cull it's face to current.
                                {
                                    faces[faced_block_index] |= (Direction)(1 << ((normal_index + 3) % 6));
                                }

                                break;
                            }
                        }

                        faces[traversal_index] |= face_direction;
                    }

                    // face is occluded
                    if (traversals == 0)
                    {
                        break;
                    }

                    // if it's the first traversal and we've only made a 1x1x1 face, continue to test next axis
                    else if ((traversals == 1) && (perpendicular_normal_index == 1))
                    {
                        continue;
                    }

                    Span<int> compressed_vertices = _PackedVertexesByIteration[normal_index];
                    int traversal_component_mask = GenerationConstants.CHUNK_SIZE_MASK << traversal_normal_shift;
                    int unary_traversal_component_mask = ~traversal_component_mask;

                    // this ternary solution should probably be temporary. not sure if there's a better way, though.
                    int uv_shift = (component_index + traversal_normal_index + ((component_index == 1) && (traversal_normal_index == 2) ? 1 : 0)) % 2;

                    int compressed_uv = (TextureAtlas.Instance.GetTileDepth(BlockRegistry.Instance.GetBlockName(block.ID))
                                        << (GenerationConstants.CHUNK_SIZE_SHIFT * 2)) // z
                                       | (traversals << (GenerationConstants.CHUNK_SIZE_SHIFT * uv_shift)) // traversal component
                                       | (1 << (GenerationConstants.CHUNK_SIZE_SHIFT * ((uv_shift + 1) % 2))); // opposite component to traversal

                    uint indexes_start = (uint)(vertexes.Count * 4);

                    indexes.Add(new QuadIndexes<uint>(
                        indexes_start + 0u,
                        indexes_start + 1u,
                        indexes_start + 3u,
                        indexes_start + 1u,
                        indexes_start + 2u,
                        indexes_start + 3u
                    ));

                    vertexes.Add(new QuadVertexes<PackedVertex>(
                        new PackedVertex(
                            localPosition
                            + ((unary_traversal_component_mask & compressed_vertices[0])
                               | ((((compressed_vertices[0] >> traversal_normal_shift) * traversals) << traversal_normal_shift) & traversal_component_mask)),
                            compressed_uv & (int.MaxValue << (GenerationConstants.CHUNK_SIZE_SHIFT * 2))),
                        new PackedVertex(
                            localPosition
                            + ((unary_traversal_component_mask & compressed_vertices[1])
                               | ((((compressed_vertices[1] >> traversal_normal_shift) * traversals) << traversal_normal_shift) & traversal_component_mask)),
                            compressed_uv & (int.MaxValue << GenerationConstants.CHUNK_SIZE_SHIFT)),
                        new PackedVertex(
                            localPosition
                            + ((unary_traversal_component_mask & compressed_vertices[2])
                               | ((((compressed_vertices[2] >> traversal_normal_shift) * traversals) << traversal_normal_shift) & traversal_component_mask)),
                            compressed_uv & int.MaxValue),
                        new PackedVertex(
                            localPosition
                            + ((unary_traversal_component_mask & compressed_vertices[3])
                               | ((((compressed_vertices[3] >> traversal_normal_shift) * traversals) << traversal_normal_shift) & traversal_component_mask)),
                            compressed_uv & ~(GenerationConstants.CHUNK_SIZE_MASK << GenerationConstants.CHUNK_SIZE_SHIFT))));

                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Wrap(int value, int delta, int minVal, int maxVal)
        {
            int mod = (maxVal + 1) - minVal;
            value += delta - minVal;
            value += (1 - (value / mod)) * mod;
            return (value % mod) + minVal;
        }
    }
}
