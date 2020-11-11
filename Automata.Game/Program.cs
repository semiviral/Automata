using System.Drawing;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Serilog;
using Silk.NET.Windowing.Common;

namespace Automata.Game
{
    public class Program
    {
        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Serilog), "Logger initialized."));

            Initialize();

            AutomataWindow.Instance.Run();
        }

        private static void Initialize()
        {
            Settings.Load();

            if (Settings.Instance.SingleThreadedGeneration) BoundedSemaphorePool.Instance.ModifyPoolSize(1);
            else BoundedSemaphorePool.Instance.DefaultPoolSize();

            BoundedSemaphorePool.Instance.ExceptionOccurred += (_, exception) => Log.Error($"{exception.Message}\r\n{exception.StackTrace}");

            WindowOptions options = WindowOptions.Default;
            options.Title = "Automata";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 100);
            options.VSync = Settings.Instance.VSync ? VSyncMode.On : VSyncMode.Off;
            options.PreferredDepthBufferBits = 24;

            AutomataWindow.Instance.CreateWindow(options);
            AutomataWindow.Instance.Closing += ApplicationCloseCallback;

            BlockRegistry.Instance.LazyInitialize();

            World world = new VoxelWorld(true);
            world.SystemManager.RegisterSystem<InputSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
            world.SystemManager.RegisterSystem<RenderSystem, LastOrderSystem>(SystemRegistrationOrder.Before);
            world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);
            world.SystemManager.RegisterSystem<ChunkGenerationSystem, ChunkRegionLoaderSystem>(SystemRegistrationOrder.After);
            World.RegisterWorld("Overworld", world);

            InitializePlayerEntity(world);
        }

        private static void InitializePlayerEntity(World world)
        {
            IEntity player = new Entity();
            world.EntityManager.RegisterEntity(player);
            world.EntityManager.RegisterComponent<Translation>(player);
            world.EntityManager.RegisterComponent<Rotation>(player);

            world.EntityManager.RegisterComponent(player, new Camera
            {
                Projector = Projector.Perspective
            });

            world.EntityManager.RegisterComponent(player, new KeyboardListener
            {
                Sensitivity = 100f
            });

            world.EntityManager.RegisterComponent(player, new MouseListener
            {
                Sensitivity = 0.5f
            });

            world.EntityManager.RegisterComponent(player, new ChunkLoader
            {
                Radius = Settings.Instance.GenerationRadius
            });
        }

        private static void ApplicationCloseCallback(object sender)
        {
            BoundedSemaphorePool.Instance.Cancel();
            GLAPI.Instance.GL.Dispose();
        }
    }
}
