#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Automata;
using Automata.Core;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Rendering;
using Automata.Rendering.OpenGL;
using Automata.Singletons;
using AutomataTest.Blocks;
using AutomataTest.Chunks;
using Serilog;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;

#endregion

namespace AutomataTest
{
    internal class Program
    {
        private static readonly string _LocalDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create);

        private static IWindow _Window;
        private static GL _GL;

        private static Shader _Shader;

        //Vertex data, uploaded to the VBO.
        private static readonly Vector3[] _vertices =
        {
            new Vector3(0f, 0f, -1f),
            new Vector3(0.5f, 0f, -1f),
            new Vector3(0f, 0.5f, -1f),
            new Vector3(0.5f, 0.5f, -1f),

            // // bottom
            // new Vector3(0f, 0f, 0f),
            // new Vector3(0f, 0f, 1f),
            // new Vector3(1f, 0f, 0f),
            // new Vector3(1f, 0f, 1f),
            //
            // // north
            // new Vector3(0f, 0f, 1f),
            // new Vector3(1f, 0f, 1f),
            // new Vector3(0f, 1f, 1f),
            // new Vector3(1f, 1f, 1f),
            //
            // // east
            // new Vector3(1f, 0f, 1f),
            // new Vector3(1f, 0f, 0f),
            // new Vector3(1f, 1f, 0f),
            // new Vector3(1f, 1f, 1f),
            //
            // // south
            // new Vector3(1f, 0f, 0f),
            // new Vector3(0f, 0f, 0f),
            // new Vector3(0f, 1f, 0f),
            // new Vector3(1f, 1f, 0f),
            //
            // // west
            // new Vector3(0f, 0f, 0f),
            // new Vector3(0f, 0f, 1f),
            // new Vector3(0f, 1f, 0f),
            // new Vector3(0f, 1f, 1f),
            //
            // // up
            // new Vector3(0f, 1f, 0f),
            // new Vector3(0f, 1f, 1f),
            // new Vector3(1f, 1f, 0f),
            // new Vector3(1f, 1f, 1f),
        };

        private static readonly uint[] _indices =
        {
            0,
            2,
            1,
            2,
            3,
            1,

            // 0,
            // 2,
            // 1,
            // 2,
            // 3,
            // 1,
            //
            // 0,
            // 2,
            // 1,
            // 2,
            // 3,
            // 1,
            //
            // 0,
            // 2,
            // 1,
            // 2,
            // 3,
            // 1,
            //
            // 0,
            // 2,
            // 1,
            // 2,
            // 3,
            // 1,
            //
            // 0,
            // 2,
            // 1,
            // 2,
            // 3,
            // 1,
        };

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Information("Static logger initialized.");

            if (!Directory.Exists($@"{_LocalDataPath}/Wyd/"))
            {
                Log.Information("Local data folder missing, creating...");
                Directory.CreateDirectory($@"{_LocalDataPath}/Wyd/");
            }

            WindowOptions options = WindowOptions.Default;
            options.Title = "Wyd: A Journey";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 400);

            _Window = Window.Create(options);
            _Window.Closing += OnClose;

            _Window.Initialize();
            Initialize();

            _Window.VSync = VSyncMode.Off;

            while (!_Window.IsClosing)
            {
                _Window.DoEvents();

                if (!_Window.IsClosing)
                {
                    World.GlobalUpdate();
                }
            }
        }

        private static void InitializeSingletons()
        {
            Singleton.InstantiateSingleton<GLAPI>();

            Singleton.InstantiateSingleton<Diagnostics>();
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("NoiseRetrieval");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("TerrainGeneration");

            Singleton.InstantiateSingleton<GameWindow>();
            GameWindow.Instance.Window = _Window;

            Singleton.InstantiateSingleton<Input>();

            Singleton.InstantiateSingleton<BlockRegistry>();
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
            world.SystemManager.RegisterSystem<ViewDoUpdateSystem, FirstOrderSystem>();
            world.SystemManager.RegisterSystem<ViewDoRenderSystem, LastOrderSystem>();
            world.SystemManager.RegisterSystem<ChunkBuildingSystem, DefaultOrderSystem>();
            World.RegisterWorld("core", world);
        }

        private static void InitializeWorldEntity(World world, out IEntity gameEntity)
        {
            gameEntity = new Entity();
            world.EntityManager.RegisterEntity(gameEntity);
            world.EntityManager.RegisterComponent(gameEntity, new PendingMeshDataComponent
            {
                Vertexes = _vertices,
                Indexes = _indices
            });
        }

        private static void InitializePlayerEntity(World world, out IEntity playerEntity)
        {
            playerEntity = new Entity();
            world.EntityManager.RegisterEntity(playerEntity);
            world.EntityManager.RegisterComponent(playerEntity, new Translation
            {
                Value = new Vector3(0f, 0f, -1.9f)
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

            InitializeWorldEntity(world, out IEntity gameEntity);

            InitializePlayerEntity(world, out IEntity playerEntity);

            Entity chunk = new Entity();
            world.EntityManager.RegisterEntity(chunk);
            world.EntityManager.RegisterComponent<Translation>(chunk);
            world.EntityManager.RegisterComponent<BlocksCollection>(chunk);
            world.EntityManager.RegisterComponent<GenerationState>(chunk);
        }

        private static Matrix4x4 _View;
        private static Matrix4x4 _Projection;
        private static readonly Glfw _glfw = Glfw.GetApi();

        private static void OnClose()
        {
            //SystemManager.Destroy();
        }
    }
}
