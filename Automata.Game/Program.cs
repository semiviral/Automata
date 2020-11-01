using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Font;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;
using Automata.Engine.Worlds;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using ConcurrencyPools;
using Serilog;
using Silk.NET.Windowing.Common;

namespace Automata.Game
{
    public class Program
    {
        private static readonly string _LocalDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}\Automata\";

        private static unsafe void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Debug("Logger initialized.");



            FontLibrary library = new FontLibrary();
            FontFace fontFace = new FontFace(library, @".\Resources\Fonts\Consolas.ttf", 0);
            fontFace.SetPixelSize(0u, 48u);

            Span<byte> memory = stackalloc byte[8192];
            byte referenceByte = memory.GetPinnableReference();

            IntPtr buffer = (IntPtr)(&referenceByte);

            uint length = 0;
            FreeType.FT_Load_Sfnt_Table(fontFace.Handle, 0u, 0, buffer, ref length);

            // if (!Directory.Exists(_LocalDataPath))
            // {
            //     Log.Information("Application data folder missing. Creating.");
            //     Directory.CreateDirectory(_LocalDataPath);
            // }

            Initialize();

            AutomataWindow.Instance.Run();
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
                Radius = 3
            });
        }

        private static void Initialize()
        {
            BoundedAsyncPool.SetActivePool();
            BoundedPool.Active.DefaultThreadPoolSize();
            BoundedPool.Active.ExceptionOccurred += (_, exception) => Log.Error($"{exception.Message}\r\n{exception.StackTrace}");

            WindowOptions options = WindowOptions.Default;
            options.Title = "Automata";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 100);
            options.VSync = VSyncMode.Off;
            options.PreferredDepthBufferBits = 24;

            AutomataWindow.Instance.CreateWindow(options);
            AutomataWindow.Instance.Closing += ApplicationCloseCallback;

            BlockRegistry.Instance.LazyInitialize();

            World world = new GameWorld(true);
            world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);
            world.SystemManager.RegisterSystem<ChunkGenerationSystem, ChunkRegionLoaderSystem>(SystemRegistrationOrder.After);
            World.RegisterWorld("core", world);

            InitializePlayerEntity(world);
        }

        private static void ApplicationCloseCallback(object sender)
        {
            BoundedPool.Active.Stop();
            GLAPI.Instance.GL.Dispose();
        }
    }
}
