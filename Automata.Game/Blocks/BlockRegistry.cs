#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine;
using Automata.Engine.Extensions;
using Serilog;

#endregion


// ReSharper disable MemberCanBePrivate.Global

namespace Automata.Game.Blocks
{
    public class BlockRegistry : Singleton<BlockRegistry>
    {
        public static ushort NullID;
        public static ushort AirID;

        private readonly Dictionary<BlockDefinition.Property, HashSet<ushort>> _PropertyBuckets;

        public List<IBlockDefinition> BlockDefinitions { get; }
        public Dictionary<string, ushort> BlockNamesByID { get; }

        public BlockRegistry()
        {
            AssignSingletonInstance(this);

            Log.Information($"({nameof(BlockRegistry)}) Creating property buckets.");

            _PropertyBuckets = EnumExtensions.GetValues<BlockDefinition.Property>().ToDictionary(val => val, _ => new HashSet<ushort>());

            BlockDefinitions = new List<IBlockDefinition>();
            BlockNamesByID = new Dictionary<string, ushort>();

            NullID = RegisterBlockDefinition("null", null);
            AirID = RegisterBlockDefinition("air", null, BlockDefinition.Property.Transparent);
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
        public ushort RegisterBlockDefinition(string blockName, Func<Direction, string>? uvsRule, params BlockDefinition.Property[] properties)
        {
            if (string.IsNullOrWhiteSpace(blockName)) throw new ArgumentException("Argument cannot be empty.", nameof(blockName));
            else if (BlockDefinitions.Count >= ushort.MaxValue) throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");

            ushort blockId = (ushort)BlockDefinitions.Count;
            blockName = blockName.ToLowerInvariant();
            uvsRule ??= _ => blockName;

            BlockDefinition blockDefinition = new BlockDefinition(blockId, blockName, uvsRule, properties);

            BlockDefinitions.Add(blockDefinition);
            BlockNamesByID.Add(blockName, blockId);

            // sort properties into buckets
            foreach (BlockDefinition.Property property in blockDefinition.Properties.GetFlags()) _PropertyBuckets[property].Add(blockDefinition.ID);

            Log.Debug($"({nameof(BlockRegistry)}) Registered ID {blockId}: '{blockName}'");

            return blockDefinition.ID;
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

        public ushort GetBlockID(string blockName) => BlockNamesByID[blockName];

        public bool TryGetBlockID(string blockName, [MaybeNullWhen(false)] out ushort blockId) => BlockNamesByID.TryGetValue(blockName, out blockId);

        public bool TryGetBlockName(ushort blockId, [NotNullWhen(true)] out string? blockName)
        {
            blockName = string.Empty;

            if (!BlockIdExists(blockId)) return false;

            blockName = BlockDefinitions[blockId].BlockName;
            return true;
        }

        public IReadOnlyBlockDefinition GetBlockDefinition(ushort blockId)
        {
            if (!BlockIdExists(blockId)) throw new ArgumentException("Given block ID does not exist.", nameof(blockId));

            return BlockDefinitions[blockId];
        }

        public bool TryGetBlockDefinition(ushort blockId, [NotNullWhen(true)] out IReadOnlyBlockDefinition? blockDefinition)
        {
            if (BlockIdExists(blockId))
            {
                blockDefinition = BlockDefinitions[blockId];
                return true;
            }

            blockDefinition = null;
            return false;
        }

        public HashSet<ushort> GetPropertyBucket(BlockDefinition.Property property) => _PropertyBuckets[property];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckBlockHasProperty(ushort blockId, BlockDefinition.Property property) => BlockDefinitions[blockId].Properties.HasFlag(property);
    }
}
