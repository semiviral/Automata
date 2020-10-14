﻿#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Automata;
using Automata.Components;
using Automata.Entities;
using Automata.Input;
using Automata.Rendering;
using Automata.Rendering.GLFW;
using Automata.Rendering.OpenGL;
using Automata.Rendering.Vulkan;
using Automata.Systems;
using Automata.Worlds;
using AutomataTest.Blocks;
using AutomataTest.Chunks;
using AutomataTest.Chunks.Generation;
using ConcurrentPools;
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
            AutomataWindow.Instance.Closing += OnClose;
            AutomataWindow.Instance.Closing += sender => BoundedThreadPool.Stop();

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
            world.EntityManager.RegisterComponent(playerEntity, new RenderShader
            {
                Value = Shader.LoadShader("Resources/Shaders/PackedVertexes.glsl", "Resources/Shaders/DefaultFragment.glsl")
            });
            world.EntityManager.RegisterComponent<InputListener>(playerEntity);
        }

        private static void Initialize()
        {
            BoundedThreadPool.DefaultThreadPoolSize();

            InitializeSingletons();

            InitializeBlocks();

            InitializeDefaultWorld(out World world);

            InitializeWorldEntity(world, out IEntity _);

            InitializePlayerEntity(world, out IEntity _);

            world.SystemManager.RegisterSystem<RotationTestSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);

            Entity chunk = new Entity();
            world.EntityManager.RegisterEntity(chunk);
            world.EntityManager.RegisterComponent<Translation>(chunk);
            world.EntityManager.RegisterComponent(chunk, new ChunkState
            {
                Value = GenerationState.Ungenerated
            });
            world.EntityManager.RegisterComponent<ChunkID>(chunk);
            world.EntityManager.RegisterComponent<BlocksCollection>(chunk);
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
