#region

using System;
using System.IO;
using System.Linq;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Textures;
using Automata.Engine.Systems;
using Automata.Engine.Worlds;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using ConcurrencyPools;
using Serilog;
using Silk.NET.Windowing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

#endregion


namespace Automata.Game
{
    public class Program
    {
        private static readonly string _LocalDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}\Automata\";

        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Debug("Logger initialized.");

            if (!Directory.Exists(_LocalDataPath))
            {
                Log.Information("Application data folder missing. Creating.");
                Directory.CreateDirectory(_LocalDataPath);
            }

            Initialize();

            AutomataWindow.Instance.Run();
        }

        private static void InitializeSingletons()
        {
            WindowOptions options = WindowOptions.Default;
            options.Title = "Automata";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 400);
            options.VSync = VSyncMode.Off;
            options.PreferredDepthBufferBits = 24;

            AutomataWindow.Instance.CreateWindow(options);
            AutomataWindow.Instance.Closing += ApplicationCloseCallback;
        }

        private static void InitializeBlocks()
        {
            BlockRegistry.Instance.RegisterBlockDefinition("bedrock", null,
                BlockDefinition.Property.Collideable);

            BlockRegistry.Instance.RegisterBlockDefinition("grass", null,
                BlockDefinition.Property.Collectible, BlockDefinition.Property.Collideable,
                BlockDefinition.Property.Destroyable);

            BlockRegistry.Instance.RegisterBlockDefinition("dirt", null,
                BlockDefinition.Property.Collectible, BlockDefinition.Property.Collideable,
                BlockDefinition.Property.Destroyable);

            BlockRegistry.Instance.RegisterBlockDefinition("dirt_coarse", null,
                BlockDefinition.Property.Collectible, BlockDefinition.Property.Collideable,
                BlockDefinition.Property.Destroyable);

            BlockRegistry.Instance.RegisterBlockDefinition("stone", null,
                BlockDefinition.Property.Collectible, BlockDefinition.Property.Collideable,
                BlockDefinition.Property.Destroyable);

            BlockRegistry.Instance.RegisterBlockDefinition("glass", null,
                BlockDefinition.Property.Transparent, BlockDefinition.Property.Collectible,
                BlockDefinition.Property.Collideable, BlockDefinition.Property.Destroyable);

            BlockRegistry.Instance.RegisterBlockDefinition("coal_ore", null,
                BlockDefinition.Property.Collectible, BlockDefinition.Property.Collideable,
                BlockDefinition.Property.Destroyable);
        }

        private static void InitializeDefaultWorld(out World world)
        {
            world = new GameWorld(true);
            world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);
            world.SystemManager.RegisterSystem<ChunkGenerationSystem, ChunkRegionLoaderSystem>(SystemRegistrationOrder.After);
            World.RegisterWorld("core", world);
        }

        private static void InitializePlayerEntity(World world)
        {
            IEntity player = new Entity();
            world.EntityManager.RegisterEntity(player);
            world.EntityManager.RegisterComponent<Translation>(player);
            world.EntityManager.RegisterComponent<Rotation>(player);
            world.EntityManager.RegisterComponent<Camera>(player);

            world.EntityManager.RegisterComponent(player, new KeyboardListener
            {
                Sensitivity = 400f
            });

            world.EntityManager.RegisterComponent(player, new MouseListener
            {
                Sensitivity = 40f
            });

            world.EntityManager.RegisterComponent(player, new ChunkLoader
            {
                Radius = 2
            });
        }

        private static void Initialize()
        {
            BoundedAsyncPool.SetActivePool();
            BoundedPool.Active.DefaultThreadPoolSize();

            InitializeSingletons();

            InitializeBlocks();

            InitializeDefaultWorld(out World world);

            InitializePlayerEntity(world);

            Atlas atlas = Atlas.Load(@".\Resources\Textures\Core\Metadata.json");
            Texture2DArray<Rgba32> blocks = new Texture2DArray<Rgba32>(8u, 8u, (uint)(atlas.Tiles?.Length ?? 0), Texture.WrapMode.Repeat,
                Texture.FilterMode.Point);

           string path = Path.Combine(@".\Resources\Textures\Core\", atlas.RelativeImagePath);
            using Image<Rgba32> image = Image.Load<Rgba32>(path);

            if (atlas.Tiles is null) return;

            int depth = 0;
            foreach (Atlas.AtlasTile tile in atlas.Tiles.Where(tile => tile is not null))
            {
                Image<Rgba32> slice = new Image<Rgba32>(8, 8);

                for (int y = 0; y < slice.Height; y++)
                for (int x = 0; x < slice.Width; x++)
                {
                    int yOffset = (tile.Offset.Y * 8) + y;
                    int xOffset = (tile.Offset.X * 8) + x;
                    slice[y, x] = image[yOffset, xOffset];
                }

                blocks.SetPixels(new Vector3i(0, 0, depth), new Vector2i(8, 8), ref slice.GetPixelRowSpan(0)[0]);

                depth += 1;

                Log.Debug($"Processed texture {tile.Name}");
            }
        }

        private static void ApplicationCloseCallback(object sender)
        {
            BoundedPool.Active.Stop();
            GLAPI.Instance.GL.Dispose();
        }
    }
}
