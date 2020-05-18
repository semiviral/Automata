#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Automata;
using Automata.Extensions;
using Serilog;

#endregion

// ReSharper disable MemberCanBePrivate.Global

namespace AutomataTest.Blocks
{
    public class BlockRegistry : Singleton<BlockRegistry>
    {
        public static ushort NullID;
        public static ushort AirID;

        private List<BlockDefinition.Property> _BlockPropertiesCache;
        private Dictionary<BlockDefinition.Property, HashSet<ushort>> _PropertiesBuckets;

        public readonly Dictionary<string, ushort> BlockNamesByID;
        public readonly List<IBlockDefinition> BlockDefinitions;

        public BlockRegistry()
        {
            AssignSingletonInstance(this);

            _BlockPropertiesCache = new List<BlockDefinition.Property>(EnumExtensions.GetEnumsList<BlockDefinition.Property>());
            _PropertiesBuckets = new Dictionary<BlockDefinition.Property, HashSet<ushort>>();

            BlockNamesByID = new Dictionary<string, ushort>();
            BlockDefinitions = new List<IBlockDefinition>();

            InitializeBlockPropertiesBuckets();

            RegisterBlockDefinition("null", null);
            RegisterBlockDefinition("air", null, BlockDefinition.Property.Transparent);

            TryGetBlockId("null", out NullID);
            TryGetBlockId("air", out AirID);
        }

        private void InitializeBlockPropertiesBuckets()
        {
            _PropertiesBuckets = new Dictionary<BlockDefinition.Property, HashSet<ushort>>();

            Log.Information($"({nameof(BlockRegistry)}) Creating property buckets.");

            foreach (BlockDefinition.Property property in EnumExtensions.GetEnumsList<BlockDefinition.Property>())
            {
                _PropertiesBuckets.Add(property, new HashSet<ushort>());
            }
        }

        private void SortBlockDefinitionPropertiesToBuckets(BlockDefinition blockDefinition)
        {
            foreach (BlockDefinition.Property property in EnumExtensions.GetEnumsList<BlockDefinition.Property>())
            {
                if (blockDefinition.HasProperty(property))
                {
                    _PropertiesBuckets[property].Add(blockDefinition.Id);
                }
            }
        }

        /// <summary>
        ///     Registers a new <see cref="BlockDefinition" /> with the given parameters.
        /// </summary>
        /// <param name="blockName">
        ///     Friendly name for <see cref="BlockDefinition" />.
        ///     remark: This value is automatically lowercased upon registration.
        /// </param>
        /// <param name="uvsRule">Optional function to return custom textures for <see cref="BlockDefinition" />.</param>
        /// <param name="properties">
        ///     Optional <see cref="BlockDefinition.Property" />s to full qualify the <see cref="BlockDefinition" />.
        /// </param>
        public void RegisterBlockDefinition(string blockName, Func<Direction, string>? uvsRule, params BlockDefinition.Property[] properties)
        {
            if (string.IsNullOrWhiteSpace(blockName))
            {
                throw new ArgumentException("Argument cannot be empty.", nameof(blockName));
            }
            else if (BlockDefinitions.Count >= ushort.MaxValue)
            {
                throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");
            }

            ushort blockId = (ushort)BlockDefinitions.Count;
            blockName = blockName.ToLowerInvariant();
            uvsRule ??= direction => blockName;

            BlockDefinition blockDefinition = new BlockDefinition(blockId, blockName, uvsRule, properties);

            BlockDefinitions.Add(blockDefinition);
            BlockNamesByID.Add(blockName, blockId);
            SortBlockDefinitionPropertiesToBuckets(blockDefinition);

            Log.Information($"({nameof(BlockRegistry)}) Registered ID {blockId}: '{blockName}'");
        }

        // public bool GetUVs(ushort blockId, Direction direction, out ushort textureId)
        // {
        //     if (!BlockIdExists(blockId))
        //     {
        //         throw new ArgumentOutOfRangeException(nameof(blockId), "Block ID does not exist.");
        //     }
        //
        //     BlockDefinitions[blockId].GetUVs(direction, out string textureName);
        //
        //     if (!TextureController.Current.TryGetTextureId(textureName, out textureId))
        //     {
        //         textureId = 0;
        //         return false;
        //     }
        //     else
        //     {
        //         return true;
        //     }
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BlockIdExists(ushort blockId) => blockId < BlockDefinitions.Count;

        public bool TryGetBlockId(string blockName, out ushort blockId)
        {
            blockId = 0;

            if (!BlockNamesByID.TryGetValue(blockName, out blockId))
            {
                Log.Warning($"({nameof(BlockRegistry)}) Failed to return block id for '{blockName}': block does not exist.");

                return false;
            }

            return true;
        }

        public bool TryGetBlockName(ushort blockId, out string blockName)
        {
            blockName = string.Empty;

            if (!BlockIdExists(blockId))
            {
                return false;
            }

            blockName = BlockDefinitions[blockId].BlockName;
            return true;
        }

        public IReadOnlyBlockDefinition GetBlockDefinition(ushort blockId)
        {
            if (!BlockIdExists(blockId))
            {
                throw new ArgumentException("Given block ID does not exist.", nameof(blockId));
            }

            return BlockDefinitions[blockId];
        }

        public bool TryGetBlockDefinition(ushort blockId, out IReadOnlyBlockDefinition? blockDefinition)
        {
            if (BlockIdExists(blockId))
            {
                blockDefinition = BlockDefinitions[blockId];
                return true;
            }

            blockDefinition = default;
            return false;
        }

        public HashSet<ushort> GetPropertyBucket(BlockDefinition.Property property) => _PropertiesBuckets[property];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckBlockHasProperty(ushort blockId, BlockDefinition.Property property) =>
            (BlockDefinitions[blockId].Properties & property) == property;
    }
}
