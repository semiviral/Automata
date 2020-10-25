#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.Vulkan;
using Automata.Engine.Systems;
using Automata.Engine.Worlds;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using ConcurrentPools;
using Serilog;
using Silk.NET.Windowing.Common;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

#endregion


namespace Automata.Game
{
    [RPlotExporter]
    public class Benchmark
    {
        private Plane[] _PlanesA;
        private Plane[] _PlanesB;
        private Cube _Cube;

        [GlobalSetup]
        public void Setup()
        {
            _Cube = new Cube(Vector3.Zero, Vector3.One);
            _PlanesA = new Plane[ClipFrustum.PLANES_SPAN_LENGTH];
            _PlanesB = new Plane[ClipFrustum.PLANES_SPAN_LENGTH];
        }

        [Benchmark]
        public Frustum.Intersect IntersectBoxUnrolled()
        {
            ClipFrustum clipFrustum = new ClipFrustum(new Span<Plane>(_PlanesA), Matrix4x4.Identity);
            return clipFrustum.BoxWithin(_Cube);
        }

        [Benchmark]
        public Frustum.Intersect IntersectBoxForeach()
        {
            ClipFrustum clipFrustum = new ClipFrustum(new Span<Plane>(_PlanesB), Matrix4x4.Identity);
            return clipFrustum.BoxWithinForeach(_Cube);
        }
    }

    public class Program
    {
        private static readonly string _LocalDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}\Automata\";

        private static void Main()
        {
            Summary? summary = BenchmarkRunner.Run<Benchmark>();

            Console.ReadKey();





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
        }

        private static void ApplicationCloseCallback(object sender)
        {
            if (VKAPI.TryValidate()) VKAPI.Instance.DestroyVulkanInstance();

            BoundedThreadPool.Stop();
        }
    }
}
