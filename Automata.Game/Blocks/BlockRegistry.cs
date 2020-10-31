#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine;
using Automata.Game.Resources;
using Serilog;

#endregion


// ReSharper disable MemberCanBePrivate.Global

namespace Automata.Game.Blocks
{
    public class BlockRegistry : Singleton<BlockRegistry>
    {
        private static readonly IReadOnlyDictionary<string, Block.Attribute> _AttributeAliases = new Dictionary<string, Block.Attribute>
        {
            { "Default", Block.Attribute.Collectible | Block.Attribute.Collideable | Block.Attribute.Destructible }
        };

        public static ushort AirID;
        public static ushort NullID;

        public List<IBlock> Blocks { get; }
        public Dictionary<string, ushort> BlockNames { get; }

        public BlockRegistry()
        {
            Blocks = new List<IBlock>();
            BlockNames = new Dictionary<string, ushort>();

            List<(string group, string path)> paths = LoadMetadata().ToList();
            TextureAtlas.Instance.Initialize(paths);
        }

        private IEnumerable<(string group, string path)> LoadMetadata()
        {
            string[] metadataFiles = Directory.GetFiles(@".\Resources\", "Metadata.json", SearchOption.AllDirectories);
            List<(string, Resource)> resources = metadataFiles.Select(path => (Path.GetDirectoryName(path) ?? String.Empty, Resource.Load(path))).ToList();

            foreach ((string directoryPath, Resource resource) in resources)
            {
                if (resource.Group is null) continue;

                foreach (Resource.BlockDefinition blockDefinition in resource.BlockDefinitions ?? Enumerable.Empty<Resource.BlockDefinition>())
                {
                    if (blockDefinition.Name is null) continue;

                    Block.Attribute attributes = 0;
                    if (blockDefinition.Attributes is not null && !TryParseAttributes(blockDefinition.Attributes, out attributes)) continue;

                    ushort id = RegisterBlock(resource.Group, blockDefinition.Name, null, attributes);

                    if (resource.Group.Equals("Core"))
                    {
                        switch (blockDefinition.Name)
                        {
                            case "Null":
                                NullID = id;
                                break;
                            case "Air":
                                AirID = id;
                                break;
                        }
                    }

                    foreach (string textureName in blockDefinition.Textures ?? Enumerable.Empty<string>())
                    {
                        string fileName = $"{(textureName.Equals("Self") ? blockDefinition.Name : textureName)}.png";
                        yield return (resource.Group, Path.Combine(directoryPath, resource.RelativeTexturesPath ?? string.Empty, fileName));
                    }
                }
            }
        }

        private static bool TryParseAttributes(IEnumerable<string> attributes, [MaybeNullWhen(false)] out Block.Attribute result)
        {
            result = (Block.Attribute)0;

            foreach (string attribute in attributes)
            {
                if (attribute.StartsWith("Alias"))
                {
                    string aliasName = attribute.Substring(attribute.IndexOf(' ') + 1);

                    if (_AttributeAliases.TryGetValue(aliasName, out Block.Attribute aliasAttribute)) result |= aliasAttribute;
                    else
                    {
                        Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(BlockRegistry),
                            $"Failed to parse {nameof(Block.Attribute)}: alias \"{aliasName}\" does not exist."));

                        return false;
                    }
                }
                else if (Enum.TryParse(typeof(Block.Attribute), attribute, true, out object? parsed)) result |= (Block.Attribute)parsed!;
                else
                {
                    Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(BlockRegistry),
                        $"Failed to parse {nameof(Block.Attribute)}: attribute \"{attribute}\" does not exist."));

                    return false;
                }
            }

            return true;
        }

        public ushort RegisterBlock(string group, string blockName, Func<Direction, string>? uvsRule, Block.Attribute attributes)
        {
            const string group_with_block_name_format = "{0}:{1}";

            if (Blocks.Count >= ushort.MaxValue) throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");

            ushort blockID = (ushort)Blocks.Count;
            string groupedName = string.Format(group_with_block_name_format, group, blockName);

            IBlock block = new Block(blockID, groupedName, uvsRule, attributes);

            Blocks.Add(block);
            BlockNames.Add(groupedName, blockID);

            Log.Debug($"({nameof(BlockRegistry)}) Registered ID {blockID}: \"{groupedName}\"");

            return block.ID;
        }

        public bool BlockIdExists(ushort blockId) => blockId < Blocks.Count;

        public ushort GetBlockID(string blockName) => BlockNames[blockName];

        public bool TryGetBlockID(string blockName, [MaybeNullWhen(false)] out ushort blockId) => BlockNames.TryGetValue(blockName, out blockId);

        public bool TryGetBlockName(ushort blockId, [NotNullWhen(true)] out string? blockName)
        {
            blockName = string.Empty;

            if (!BlockIdExists(blockId)) return false;

            blockName = Blocks[blockId].BlockName;
            return true;
        }

        public IBlock GetBlockDefinition(ushort blockId)
        {
            if (!BlockIdExists(blockId)) throw new ArgumentException("Given block ID does not exist.", nameof(blockId));

            return Blocks[blockId];
        }

        public bool TryGetBlockDefinition(ushort blockId, [NotNullWhen(true)] out IBlock? blockDefinition)
        {
            if (BlockIdExists(blockId))
            {
                blockDefinition = Blocks[blockId];
                return true;
            }

            blockDefinition = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckBlockHasProperty(ushort blockId, Block.Attribute attribute) => Blocks[blockId].HasAttribute(attribute);
    }
}
