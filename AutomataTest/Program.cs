#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Automata;
using Automata.Entity;
using Automata.Input;
using Automata.Rendering;
using Automata.Rendering.GLFW;
using Automata.Rendering.Meshes;
using Automata.Rendering.OpenGL;
using Automata.Rendering.Vulkan;
using Automata.System;
using Automata.Worlds;
using AutomataTest.Blocks;
using AutomataTest.Chunks.Generation;
using Serilog;
using Serilog.Events;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;

#endregion

namespace AutomataTest
{
    internal class Program
    {
        private static readonly string _LocalDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}/Automata/";

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Is(
#if DEBUG
                LogEventLevel.Verbose
#else
                LogEventLevel.Information
#endif
            ).CreateLogger();
            Log.Information("Static logger initialized.");

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
            AutomataWindow.Instance.Initialize();
            AutomataWindow.Instance.Closing += OnClose;

            Singleton.CreateSingleton<GLAPI>();

            Singleton.CreateSingleton<Diagnostics>();
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("NoiseRetrieval");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("TerrainGeneration");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("PreMeshing");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("Meshing");

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

            world.SystemManager.RegisterSystem<ChunkGenerationSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);
            World.RegisterWorld("core", world);
        }

        private static void InitializeWorldEntity(World world, out IEntity gameEntity)
        {
            gameEntity = new Entity();
            world.EntityManager.RegisterEntity(gameEntity);
        }

        private static void InitializePlayerEntity(World world, out IEntity playerEntity)
        {
            playerEntity = new Entity();
            world.EntityManager.RegisterEntity(playerEntity);
            world.EntityManager.RegisterComponent(playerEntity, new Translation
            {
                Value = new Vector3(0f, 0f, -3f)
            });
            world.EntityManager.RegisterComponent<Rotation>(playerEntity);
            world.EntityManager.RegisterComponent<Camera>(playerEntity);
            world.EntityManager.RegisterComponent<InputListener>(playerEntity);
        }

        private static void Initialize()
        {
            InitializeSingletons();

            InitializeBlocks();

            InitializeDefaultWorld(out World world);

            InitializeWorldEntity(world, out IEntity _);

            InitializePlayerEntity(world, out IEntity _);

            world.SystemManager.RegisterSystem<RotationTestSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);

            Mesh<float> mesh = new Mesh<float>();
            mesh.VertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0);
            mesh.VertexesBuffer.SetBufferData(StaticCube.Vertexes);
            mesh.IndexesBuffer.SetBufferData(StaticCube.Indexes);

            const int diameter = 4;
            const float tile_factor = 3f;
            for (int x = 0; x < diameter; x++)
            for (int y = 0; y < diameter; y++)
            for (int z = 0; z < diameter; z++)
            {
                Entity cube = new Entity();
                world.EntityManager.RegisterEntity(cube);
                world.EntityManager.RegisterComponent(cube, new Translation
                {
                    Value = new Vector3(x * tile_factor, y * tile_factor, z * tile_factor)
                });
                world.EntityManager.RegisterComponent<Rotation>(cube);
                world.EntityManager.RegisterComponent<RotationTest>(cube);
                world.EntityManager.RegisterComponent(cube, new RenderMesh
                {
                    Mesh = mesh
                });
            }

            // Entity chunk = new Entity();
            // world.EntityManager.RegisterEntity(chunk);
            // world.EntityManager.RegisterComponent<Translation>(chunk);
            // world.EntityManager.RegisterComponent(chunk, new ChunkState
            // {
            //     Value = GenerationState.Unbuilt
            // });
            // world.EntityManager.RegisterComponent<ChunkID>(chunk);
            // world.EntityManager.RegisterComponent<BlocksCollection>(chunk);
        }

        private static void OnClose(object sender)
        {
            if (VKAPI.TryValidate())
            {
                VKAPI.Instance.DestroyVulkanInstance();
            }
        }
    }
}
