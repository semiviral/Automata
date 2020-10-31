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
            public string? Name { get; set; }
            public List<string>? Attributes { get; set; }
            public List<string>? Textures { get; set; }
        }

        public string? Group { get; set; }
        public string? RelativeTexturesPath { get; set; }
        public List<BlockDefinition>? BlockDefinitions { get; set; }

        public static Resource Load(string path) => JsonSerializer.Deserialize<Resource>(File.ReadAllText(path));
    }
}
