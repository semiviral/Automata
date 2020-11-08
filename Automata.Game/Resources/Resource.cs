using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Automata.Game.Resources
{
    public class Resource
    {
        public class BlockDefinition
        {
            public class LightEmission
            {
                public class LowColor15
                {
                    public byte R { get; set; }
                    public byte G { get; set; }
                    public byte B { get; set; }
                }

                public byte Intensity { get; set; }
                public LowColor15 Color { get; set; }
            }

            public string? Name { get; set; }
            public List<string>? Attributes { get; set; }
            public string? MeshingStrategy { get; set; }
            public List<string>? Textures { get; set; }
            public LightEmission? Luminance { get; set; }

            public BlockDefinition() => MeshingStrategy = "Block";
        }

        public string? Group { get; set; }
        public string? RelativeTexturesPath { get; set; }
        public List<BlockDefinition>? BlockDefinitions { get; set; }

        public static Resource Load(string path) => JsonSerializer.Deserialize<Resource>(File.ReadAllText(path));
    }
}
