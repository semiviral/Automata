using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Automata.Game.Blocks;

namespace Automata.Game.Resources
{
    public class Resource
    {
        public class BlockDefinition
        {
            private string? _Name;

            public string? Name { get => _Name; set => _Name = value?.ToLowerInvariant(); }
            public List<string>? Attributes { get; set; }
            public List<string>? Textures { get; set; }
        }

        private string? _Group;
        private string? _RelativeTexturesPath;

        public string? Group { get => _Group; set => _Group = value?.ToLowerInvariant(); }
        public string? RelativeTexturesPath { get => _RelativeTexturesPath; set => _RelativeTexturesPath = value?.ToLowerInvariant(); }
        public List<BlockDefinition>? BlockDefinitions { get; set; }

        public static Resource Load(string path) => JsonSerializer.Deserialize<Resource>(File.ReadAllText(path));
    }
}
