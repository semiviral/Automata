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

// ReSharper disable MemberCanBePrivate.Global

namespace Automata.Game.Blocks
{
    public class BlockRegistry : Singleton<BlockRegistry>
    {
        private static readonly IReadOnlyDictionary<string, IBlockDefinition.Attribute> _AttributeAliases =
            new Dictionary<string, IBlockDefinition.Attribute>
            {
                ["Default"] = IBlockDefinition.Attribute.Collectible
                              | IBlockDefinition.Attribute.Collideable
                              | IBlockDefinition.Attribute.Destructible
            };

        public static ushort AirID;
        public static ushort NullID;

        public List<IBlockDefinition> Blocks { get; }
        public Dictionary<string, ushort> BlocksIndexer { get; }

        public BlockRegistry()
        {
            Blocks = new List<IBlockDefinition>();
            BlocksIndexer = new Dictionary<string, ushort>();

            List<(string group, string path)> paths = LoadMetadata().ToList();
            TextureAtlas.Instance.Initialize(paths);
        }

        private IEnumerable<(string group, string path)> LoadMetadata()
        {
            string[] metadata_files = Directory.GetFiles(@".\Resources\", "Metadata.json", SearchOption.AllDirectories);
            List<(string, Resource)> resources = metadata_files.Select(path => (Path.GetDirectoryName(path) ?? String.Empty, Resource.Load(path))).ToList();

            foreach ((string directory_path, Resource resource) in resources)
            {
                if (resource.Group is null)
                {
                    continue;
                }

                foreach (Resource.BlockDefinition block_definition in resource.BlockDefinitions ?? Enumerable.Empty<Resource.BlockDefinition>())
                {
                    if (block_definition.Name is null)
                    {
                        continue;
                    }

                    IBlockDefinition.Attribute attributes = 0;

                    if (block_definition.Attributes is not null && !TryParseAttributes(block_definition.Attributes, out attributes))
                    {
                        continue;
                    }

                    ushort id = RegisterBlock(resource.Group, block_definition.Name, attributes, block_definition.MeshingStrategy);

                    if (resource.Group.Equals("Core"))
                    {
                        switch (block_definition.Name)
                        {
                            case "Null":
                                NullID = id;
                                break;
                            case "Air":
                                AirID = id;
                                break;
                        }
                    }

                    foreach (string texture_name in block_definition.Textures ?? Enumerable.Empty<string>())
                    {
                        string file_name = $"{(texture_name.Equals("Self") ? block_definition.Name : texture_name)}.png";
                        yield return (resource.Group, Path.Combine(directory_path, resource.RelativeTexturesPath ?? string.Empty, file_name));
                    }
                }
            }
        }

        private bool TryParseAttributes(IEnumerable<string> attributes, [MaybeNullWhen(false)] out IBlockDefinition.Attribute result)
        {
            result = (IBlockDefinition.Attribute)0;

            foreach (string attribute in attributes)
            {
                if (attribute.StartsWith("Alias"))
                {
                    string alias_name = attribute.Substring(attribute.IndexOf(' ') + 1);

                    if (_AttributeAliases.TryGetValue(alias_name, out IBlockDefinition.Attribute alias_attribute))
                    {
                        result |= alias_attribute;
                    }
                    else
                    {
                        Log.Error(string.Format(_LogFormat,
                            $"Failed to parse {nameof(IBlockDefinition.Attribute)}: alias \"{alias_name}\" does not exist."));

                        return false;
                    }
                }
                else if (Enum.TryParse(typeof(IBlockDefinition.Attribute), attribute, true, out object? parsed))
                {
                    result |= (IBlockDefinition.Attribute)parsed!;
                }
                else
                {
                    Log.Error(string.Format(_LogFormat,
                        $"Failed to parse {nameof(IBlockDefinition.Attribute)}: attribute \"{attribute}\" does not exist."));

                    return false;
                }
            }

            return true;
        }

        public ushort RegisterBlock(string group, string blockName, IBlockDefinition.Attribute attributes, string? meshingStrategy)
        {
            const string group_with_block_name_format = "{0}:{1}";

            if (Blocks.Count >= ushort.MaxValue)
            {
                throw new OverflowException($"{nameof(BlockRegistry)} has run out of valid block IDs.");
            }

            ushort block_id = (ushort)Blocks.Count;
            string grouped_name = string.Format(group_with_block_name_format, group, blockName);

            int strategy_index = ChunkMesher.MeshingStrategies.GetMeshingStrategyIndex(meshingStrategy ?? ChunkMesher.DEFAULT_STRATEGY);
            IBlockDefinition block_definition = new BlockDefinition(block_id, grouped_name, strategy_index, attributes);

            Blocks.Add(block_definition);
            BlocksIndexer.Add(grouped_name, block_id);

            Log.Debug($"({nameof(BlockRegistry)}) Registered ID {block_id}: \"{grouped_name}\"");

            return block_definition.ID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BlockIDExists(ushort blockID) => blockID < Blocks.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetBlockID(string blockName) => BlocksIndexer[blockName];

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
        public bool CheckBlockHasProperty(ushort blockID, IBlockDefinition.Attribute attribute) => Blocks[blockID].HasAttribute(attribute);
    }
}
