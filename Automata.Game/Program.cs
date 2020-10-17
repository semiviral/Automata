#region

using System;
using System.Drawing;
using System.IO;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.Vulkan;
using Automata.Engine.Systems;
using Automata.Engine.Worlds;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using ConcurrentPools;
using Serilog;
using Serilog.Events;
using Silk.NET.Windowing.Common;

#endregion

namespace Automata.Game
{
    internal class Program
    {
        private static readonly string _LocalDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}/Automata/";

        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console().MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();
            Log.Information("Logger initialized.");

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

            Singleton.CreateSingleton<GLFWAPI>();
            Singleton.CreateSingleton<InputManager>();
            Singleton.CreateSingleton<AutomataWindow>();
            AutomataWindow.Instance.CreateWindow(options);
            AutomataWindow.Instance.Closing += ApplicationCloseCallback;

            Singleton.CreateSingleton<GLAPI>();
            Singleton.CreateSingleton<BlockRegistry>();
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
            world.EntityManager.RegisterComponent<InputListener>(player);
            world.EntityManager.RegisterComponent(player, new RenderShader
            {
                Value = Shader.LoadShader("Resources/Shaders/PackedVertexes.glsl", "Resources/Shaders/DefaultFragment.glsl")
            });
            world.EntityManager.RegisterComponent(player, new ChunkLoader
            {
                Radius = 4
            });
        }

        private static void Initialize()
        {
            BoundedThreadPool.DefaultThreadPoolSize();

            InitializeSingletons();

            InitializeBlocks();

            InitializeDefaultWorld(out World world);

            InitializePlayerEntity(world);

            // const int radius = 3;
            //
            // for (int x = -radius; x < (radius + 1); x++)
            // for (int z = -radius; z < (radius + 1); z++)
            // for (int y = 0; y < (GenerationConstants.WORLD_HEIGHT / GenerationConstants.CHUNK_SIZE); y++)
            // {
            //     Entity chunk = new Entity();
            //     world.EntityManager.RegisterEntity(chunk);
            //     world.EntityManager.RegisterComponent(chunk, new Translation
            //     {
            //         Value = new Vector3(x, y, z) * GenerationConstants.CHUNK_SIZE
            //     });
            //     world.EntityManager.RegisterComponent(chunk, new ChunkState
            //     {
            //         Value = GenerationState.Ungenerated
            //     });
            //     world.EntityManager.RegisterComponent<ChunkID>(chunk);
            //     world.EntityManager.RegisterComponent<BlocksCollection>(chunk);
            // }
        }

        private static void ApplicationCloseCallback(object sender)
        {
            if (VKAPI.TryValidate())
            {
                VKAPI.Instance.DestroyVulkanInstance();
            }

            BoundedThreadPool.Stop();
        }
    }
}
