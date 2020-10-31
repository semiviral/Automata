using System;
using System.IO;
using System.Text.Json;

namespace Automata.Engine
{
    public class Atlas
    {
        public class Tile
        {
            public class Coordinates
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            private string? _Name;
            public string? Name { get => _Name; set => _Name = value?.ToLowerInvariant(); }
            public Coordinates? Offset { get; set; }
        }

        public string? RelativeImagePath { get; set; }
        public Tile?[]? Tiles { get; set; }

        public static Atlas Load(string path)
        {
            ReadOnlySpan<byte> bytes = File.ReadAllBytes(path);
            return JsonSerializer.Deserialize<Atlas>(bytes);
        }
    }
}
