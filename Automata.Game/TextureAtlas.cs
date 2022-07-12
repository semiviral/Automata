using System;
using System.Collections.Generic;
using System.IO;
using Automata.Engine;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL.Textures;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Game
{
    public class TextureAtlas : Singleton<TextureAtlas>, IDisposable
    {
        private readonly Dictionary<string, int> _TextureDepths;

        public Texture2DArray<Rgba32>? Blocks { get; private set; }

        public TextureAtlas() => _TextureDepths = new Dictionary<string, int>();

        public void Initialize(IList<(string group, string path)> texturePaths)
        {
            const string group_with_sprite_name_format = "{0}:{1}";

            Blocks = new Texture2DArray<Rgba32>(new Vector3<int>(8, 8, texturePaths.Count), Texture.WrapMode.Repeat, Texture.FilterMode.Point);

            int depth = 0;

            foreach ((string group, string path) in texturePaths)
            {
                Blocks.SetPixels(new Vector3<int>(0, 0, depth), new Vector2<int>(8, 8), Image.Load<Rgba32>(path).GetPixelSpan());

                string formatted_name = string.Format(group_with_sprite_name_format, group, Path.GetFileNameWithoutExtension(path));

                // it shouldn't be too uncommon for multiple identical paths to be parsed out
                // as it just means multiple blocks are using the same texture
                if (_TextureDepths.ContainsKey(formatted_name))
                {
                    continue;
                }

                if (_TextureDepths.TryAdd(formatted_name, depth))
                {
                    Log.Debug(string.Format(_LogFormat, $"Registered texture: \"{formatted_name}\" depth {depth}"));
                }
                else
                {
                    Log.Warning(string.Format(_LogFormat, $"Failed to register texture: \"{formatted_name}\" depth {depth}"));
                }

                depth += 1;
            }

            Log.Debug(string.Format(_LogFormat, $"Registered {_TextureDepths.Count} textures."));
        }

        public int GetTileDepth(string tileName) => _TextureDepths[tileName];

        public void Dispose()
        {
            Blocks?.Dispose();
            _TextureDepths.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
