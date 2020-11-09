#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Automata.Engine;
using Automata.Game.Chunks.Generation.Meshing;
using Automata.Game.Resources;
using Serilog;

#endregion


// ReSharper disable MemberCanBePrivate.Global

namespace Automata.Game.Blocks
{
    public class BlockRegistry : Singleton<BlockRegistry>
    {
        private static readonly IReadOnlyDictionary<string, BlockDefinitionDefinition.Attribute> _AttributeAliases = new Dictionary<string, BlockDefinitionDefinition.Attribute>
        {
            { "Default", BlockDefinitionDefinition.Attribute.Collectible | BlockDefinitionDefinition.Attribute.Collideable | BlockDefinitionDefinition.Attribute.Destructible }
        };

        public static ushort AirID;
        public static ushort NullID;

        public List<IBlockDefinition> Blocks { get; }
        public Dictionary<string, ushort> BlockNames { get; }

        public BlockRegistry()
        {
            Blocks = new List<IBlockDefinition>();
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

                    BlockDefinitionDefinition.Attribute attributes = 0;

                    if (blockDefinition.Attributes is not null && !TryParseAttributes(blockDefinition.Attributes, out attributes)) continue;

                    ushort id = RegisterBlock(resource.Group, blockDefinition.Name, attributes, blockDefinition.MeshingStrategy);

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

        private bool TryParseAttributes(IEnumerable<string> attributes, [MaybeNullWhen(false)] out BlockDefinitionDefinition.Attribute result)
        {
            result = (BlockDefinitionDefinition.Attribute)0;

            foreach (string attribute in attributes)
            {
                if (attribute.StartsWith("Alias"))
                {
                    string aliasName = attribute.Substring(attribute.IndexOf(' ') + 1);

                    if (_AttributeAliases.TryGetValue(aliasName, out BlockDefinitionDefinition.Attribute aliasAttribute)) result |= aliasAttribute;
                    else
                    {
                        Log.Error(string.Format(_LogFormat, $"Failed to parse {nameof(BlockDefinitionDefinition.Attribute)}: alias \"{aliasName}\" does not exist."));

                        return false;
                    }
                }
                else if (Enum.TryParse(typeof(BlockDefinitionDefinition.Attribute), attribute, true, out object? parsed)) result |= (BlockDefinitionDefinition.Attribute)parsed!;
                else
                {
                    Log.Error(string.Format(_LogFormat, $"Failed to parse {nameof(BlockDefinitionDefinition.Attribute)}: attribute \"{attribute}\" does not exist."));

                    return false;
                }
            }

            return true;
        }

        public ushort RegisterBlock(string group, string blockName, BlockDefinitionDefinition.Attribute attributes, string? meshingStrategy)
        {
            const string group_with_block_name_format = "{0}:{1}";

            if (Blocks.Count >= ushort.MaxValue) throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");

            ushort blockID = (ushort)Blocks.Count;
            string groupedName = string.Format(group_with_block_name_format, group, blockName);

            int strategyIndex = ChunkMesher.MeshingStrategies.GetMeshingStrategyIndex(meshingStrategy ?? ChunkMesher.DEFAULT_STRATEGY);
            IBlockDefinition blockDefinition = new BlockDefinitionDefinition(blockID, groupedName, strategyIndex, attributes);

            Blocks.Add(blockDefinition);
            BlockNames.Add(groupedName, blockID);

            Log.Debug($"({nameof(BlockRegistry)}) Registered ID {blockID}: \"{groupedName}\"");

            return blockDefinition.ID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BlockIDExists(ushort blockID) => blockID < Blocks.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetBlockID(string blockName) => BlockNames[blockName];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetBlockName(ushort blockID) => Blocks[blockID].BlockName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBlockDefinition GetBlockDefinition(ushort blockID) => Blocks[blockID];

        public bool TryGetBlockDefinition(ushort blockID, [NotNullWhen(true)] out IBlockDefinition? blockDefinition)
        {
            if (BlockIDExists(blockID))
            {
                blockDefinition = Blocks[blockID];
                return true;
            }

            blockDefinition = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckBlockHasProperty(ushort blockID, BlockDefinitionDefinition.Attribute attribute) => Blocks[blockID].HasAttribute(attribute);
    }
}
