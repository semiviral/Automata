﻿#region

using System;
using System.Drawing;
using System.IO;
using Automata;
using Automata.GLFW;
using Automata.Input;
using Automata.Numerics;
using Automata.Rendering;
using Automata.Rendering.OpenGL;
using Automata.Rendering.Vulkan;
using Automata.Worlds;
using AutomataTest.Blocks;
using AutomataTest.Chunks;
using AutomataTest.Chunks.Generation;
using Serilog;
using Serilog.Events;
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
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Is(LogEventLevel.Verbose).CreateLogger();
            Log.Information("Static logger initialized.");

            if (!Directory.Exists(_LocalDataPath))
            {
                Log.Information("Application data folder missing.");
                Log.Information("Creating application data folder.");
                Directory.CreateDirectory(_LocalDataPath);
            }

            WindowOptions options = WindowOptions.DefaultVulkan;
            options.Title = "Wyd: A Journey";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 400);
            options.VSync = VSyncMode.Off;

            Singleton.CreateSingleton<AutomataWindow>();
            AutomataWindow.Instance.CreateWindow(options);
            AutomataWindow.Instance.Window.Closing += OnClose;
            AutomataWindow.Instance.Window.Initialize();

            Initialize();

            AutomataWindow.Instance.Run();
        }

        private static void InitializeSingletons()
        {
            Singleton.CreateSingleton<VKAPI>();
            VKAPI.Instance.DefaultInitialize();

            Singleton.CreateSingleton<Diagnostics>();
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("NoiseRetrieval");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("TerrainGeneration");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("PreMeshing");
            Diagnostics.Instance.RegisterDiagnosticTimeEntry("Meshing");

            Singleton.CreateSingleton<Input>();

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
                Value = new Vector3d(0d, 0d, 0d)
            });
            world.EntityManager.RegisterComponent<Rotation>(playerEntity);
            world.EntityManager.RegisterComponent(playerEntity, new Camera
            {
                Shader = new Shader("PackedVertexes.glsl", "DefaultFragment.glsl")
            });
            world.EntityManager.RegisterComponent<InputListener>(playerEntity);
        }

        private static void Initialize()
        {
            InitializeSingletons();

            InitializeBlocks();

            InitializeDefaultWorld(out World world);

            InitializeWorldEntity(world, out IEntity _);

            InitializePlayerEntity(world, out IEntity _);

            Entity chunk = new Entity();
            world.EntityManager.RegisterEntity(chunk);
            world.EntityManager.RegisterComponent<Translation>(chunk);
            world.EntityManager.RegisterComponent(chunk, new ChunkState
            {
                Value = GenerationState.Unbuilt
            });
            world.EntityManager.RegisterComponent<ChunkID>(chunk);
            world.EntityManager.RegisterComponent<BlocksCollection>(chunk);
        }

        private static void OnClose()
        {
            VKAPI.Instance.DestroyVulkanInstance();
            //SystemManager.Destroy();
        }
    }
}
