using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automata.Engine;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.OpenGL.Textures;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Game
{
    public class TextureAtlas : Singleton<TextureAtlas>
    {
        private const string _ATLAS_METADATA_MASK = "Atlas";

        private readonly Dictionary<string, int> _TextureDepths;

        public Texture2DArray<Rgba32>? Blocks { get; private set; }

        public TextureAtlas() => _TextureDepths = new Dictionary<string, int>();

        public void EarlyInitialize() => LoadAllTextures();

        public int GetTileDepth(string tileName) => _TextureDepths[tileName];

        public bool TryGetTileDepth(string tileName, [MaybeNullWhen(false)] out int depth)
        {
            bool success= _TextureDepths.TryGetValue(tileName, out depth);

            if (!success)
            {

            }

            return success;
        }

        private void LoadAllTextures()
        {
            List<(string DirectoryPath, Atlas Atlas)> atlases = GetAtlases().ToList();
            int tileCount = atlases.SelectMany(tuple => tuple.Atlas.Tiles).Count();
            Texture2DArray<Rgba32> blocks = new Texture2DArray<Rgba32>(8u, 8u, (uint)tileCount, Texture.WrapMode.Repeat, Texture.FilterMode.Point);
            Image<Rgba32> slice = new Image<Rgba32>(8, 8);
            int depth = 0;

            foreach ((string directoryPath, Atlas atlas) in atlases)
            {
                string atlasImagePath = Path.Combine(directoryPath, atlas.RelativeImagePath!);
                using Image<Rgba32> image = Image.Load<Rgba32>(atlasImagePath);

                foreach (Atlas.Tile? tile in atlas.Tiles!.Where(tile => tile?.Name is not null))
                {
                    int xOffset = tile!.Offset?.X * 8 ?? 0;
                    int yOffset = tile!.Offset?.Y * 8 ?? 0;

                    for (int x = 0; x < slice.Width; x++)
                    for (int y = 0; y < slice.Height; y++)
                        slice[x, y] = image[xOffset + x, yOffset + y];

                    blocks.SetPixels(new Vector3i(0, 0, depth), new Vector2i(8, 8), ref slice.GetPixelRowSpan(0)[0]);

                    if (_TextureDepths.TryAdd(tile!.Name ?? $"unnamed_{depth}", depth))
                        Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(TextureAtlas), $"Registered texture: \"{tile.Name}\" depth {depth}"));
                    else Log.Warning(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(TextureAtlas),
                        $"Failed to register texture: \"{tile.Name}\" depth {depth}"));

                    depth += 1;
                }
            }

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(TextureAtlas), $"Registered {_TextureDepths.Count} textures."));
        }

        private static IEnumerable<(string DirectoryPath, Atlas Atlas)> GetAtlases()
        {
            string[] metadataFiles = Directory.GetFiles(@".\Resources\Textures\", $"{_ATLAS_METADATA_MASK}.json", SearchOption.AllDirectories);

            foreach (string metadataFilePath in metadataFiles)
            {
                Atlas atlas = Atlas.Load(metadataFilePath);
                string? directoryPath = Path.GetDirectoryName(metadataFilePath);

                if (atlas.RelativeImagePath is null || atlas.Tiles is null || directoryPath is null) continue;

                yield return (directoryPath, atlas);
            }
        }
    }
}
