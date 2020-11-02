using System;
using System.Drawing;
using System.Linq;
using System.Text;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Fonts;
using Automata.Engine.Rendering.Fonts.FreeTypePrimitives;
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
        private static unsafe void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Debug("Logger initialized.");

            FontLibrary library = new FontLibrary();
            FontFace fontFace = new FontFace(library, @".\Resources\Fonts\Arial.ttf", 0u);
            fontFace.SetPixelSize(0u, 12u);
            fontFace.SelectCharmap(FontEncoding.Unicode);
            fontFace.ParseAvailableCharacters();

            Glyph glyph;
            for (byte charCode = 0; charCode < 128; charCode++)
            {
                fontFace.LoadCharacter((char)charCode, LoadFlags.Default);

                 glyph = fontFace.Glyph();
                Console.WriteLine(glyph.Metrics().Height.Value);
            }

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
