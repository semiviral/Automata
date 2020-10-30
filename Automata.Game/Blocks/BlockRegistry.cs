#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Enumeration;
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
            { "default", Block.Attribute.Collectible | Block.Attribute.Collideable | Block.Attribute.Destructible }
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
            
        }

        public void Initialize() { }

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

                    if (blockDefinition.Attributes is not null) attributes = ParseAttributes(blockDefinition.Attributes);

                    ushort id = RegisterBlock(resource.Group, blockDefinition.Name, null, attributes);

                    if (resource.Group.Equals("core"))
                    {
                        switch (blockDefinition.Name)
                        {
                            case "null":
                                NullID = id;
                                break;
                            case "air":
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

        private static Block.Attribute ParseAttributes(IEnumerable<string> attributes)
        {
            Block.Attribute result = (Block.Attribute)0;

            foreach (string attribute in attributes.Select(attribute => attribute.ToLowerInvariant()))
            {
                result |= attribute.StartsWith("alias")
                    ? _AttributeAliases[attribute.Substring(attribute.IndexOf(' ') + 1)]
                    : Enum.Parse<Block.Attribute>(attribute, true);
            }

            return result;
        }

        public ushort RegisterBlock(string group, string blockName, Func<Direction, string>? uvsRule, Block.Attribute attributes)
        {
            const string block_name_with_group_format = "{0}:{1}";

            if (Blocks.Count >= ushort.MaxValue) throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");

            ushort blockId = (ushort)Blocks.Count;
            group = group.ToLowerInvariant();
            blockName = blockName.ToLowerInvariant();
            uvsRule ??= _ => blockName;

            IBlock block = new Block(blockId, blockName, uvsRule, attributes);

            Blocks.Add(block);
            BlockNames.Add(string.Format(block_name_with_group_format, group, blockName), blockId);

            Log.Debug($"({nameof(BlockRegistry)}) Registered ID {blockId}: '{blockName}'");

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
