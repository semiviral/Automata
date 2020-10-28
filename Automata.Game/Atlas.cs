using System;
using System.IO;
using System.Text.Json;
using Automata.Engine.Numerics;

namespace Automata.Game
{
    public class Atlas
    {
        public class AtlasTile
        {
            public string Name { get; set; }
            public Vector2i Offset { get; set; }
        }

        public string RelativeImagePath { get; set; }
        public AtlasTile[] AtlasTiles { get; set; }

        public static Atlas Load(string path)
        {
            ReadOnlySpan<byte> bytes = File.ReadAllBytes(path);
            return JsonSerializer.Deserialize<Atlas>(bytes);
        }
    }
}
