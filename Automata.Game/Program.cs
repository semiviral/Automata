using System.Drawing;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;
using Automata.Game;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Serilog;
using Silk.NET.Windowing.Common;

Settings.Load();
InitializeLogger();
InitializeBoundedPool();
InitializeWindow();
BlockRegistry.Instance.LazyInitialize();
InitializeWorld(out World world);
InitializePlayer(out IEntity player);
world.EntityManager.RegisterEntity(player);
await AutomataWindow.Instance.Run();

static void ApplicationCloseCallback(object sender)
{
    BoundedInvocationPool.Instance.Cancel();
    GLAPI.Instance.GL.Dispose();
}


#region Initialization

static void InitializeLogger()
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger();

    Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Serilog), "Logger initialized."));
}

static void InitializeBoundedPool()
{
    if (Settings.Instance.SingleThreadedGeneration) BoundedInvocationPool.Instance.ModifyPoolSize(1);
    else BoundedInvocationPool.Instance.DefaultPoolSize();

    BoundedInvocationPool.Instance.ExceptionOccurred += (_, exception) => Log.Error($"{exception.Message}\r\n{exception.StackTrace}");
}

static void InitializeWindow()
{
    WindowOptions options = WindowOptions.Default;
    options.Title = "Automata";
    options.Size = new Size(800, 600);
    options.Position = new Point(500, 100);
    options.VSync = Settings.Instance.VSync ? VSyncMode.On : VSyncMode.Off;
    options.PreferredDepthBufferBits = 24;

    AutomataWindow.Instance.CreateWindow(options);
    AutomataWindow.Instance.Closing += ApplicationCloseCallback;
}

static void InitializeWorld(out World world)
{
    world = new VoxelWorld(true);
    world.SystemManager.RegisterSystem<InputSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<RenderSystem, LastOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.After);
    world.SystemManager.RegisterSystem<ChunkGenerationSystem, ChunkRegionLoaderSystem>(SystemRegistrationOrder.After);
    world.SystemManager.RegisterSystem<ChunkModificationsSystem, ChunkGenerationSystem>(SystemRegistrationOrder.Before);
    World.RegisterWorld("Overworld", world);
}

static void InitializePlayer(out IEntity player)
{
    player = new Entity
    {
        new Translation(),
        new Rotation(),
        new Camera
        {
            Projector = Projector.Perspective
        },
        new KeyboardListener
        {
            Sensitivity = 100f
        },
        new MouseListener
        {
            Sensitivity = 0.5f
        },
        new ChunkLoader
        {
#if DEBUG
            Radius = 4
#else
            Radius = Settings.Instance.GenerationRadius
#endif
        }
    };
}

#endregion
