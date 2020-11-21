using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Automata.Engine;
using Automata.Engine.Concurrency;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Game;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Automata.Game.Chunks.Generation.Meshing;
using Serilog;
using Silk.NET.Windowing.Common;

MainImpl();
await AutomataWindow.Instance.RunAsync();


#region Main

void MainImpl()
{
    Settings.Load();
    InitializeLoggerImpl();
    InitializeBoundedPoolImpl();
    InitializeWindowImpl();

#if VULKAN
    VulkanInstance instance = VKAPI.Instance.GenerateNewInstance(AutomataWindow.Instance.GetSurface());
    SurfaceExtension surfaceExtension = instance.GetInstanceExtension<SurfaceExtension>();
    VulkanDebugMessenger debugMessenger = new VulkanDebugMessenger(instance);
#else
    BlockRegistry.Instance.LazyInitialize();
    InitializeWorldImpl(out World world);
    InitializePlayerImpl(world.EntityManager);
#endif
}

static void ApplicationCloseCallbackImpl(object sender)
{
    World.DisposeWorlds();
    ProgramRegistry.Instance.Dispose();
    TextureAtlas.Instance.Blocks?.Dispose();
    BoundedInvocationPool.Instance.Cancel();
    AutomataWindow.Instance.Dispose();
}

#endregion


#region Initialization

static void InitializeLoggerImpl()
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.Async(config => config.File("log.txt"))
        .CreateLogger();

    Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Serilog), "Logger initialized."));
}

static void InitializeBoundedPoolImpl()
{
    if (Settings.Instance.SingleThreadedGeneration)
    {
        BoundedInvocationPool.Instance.ModifyPoolSize(1);
    }
    else
    {
        BoundedInvocationPool.Instance.DefaultPoolSize();
    }

    BoundedInvocationPool.Instance.ExceptionOccurred += (_, exception) => Log.Error($"{exception.Message}\r\n{exception.StackTrace}");
}

static void InitializeWindowImpl()
{
    WindowOptions options = WindowOptions.Default;
    options.Title = "Automata";
    options.Size = new Size(800, 600);
    options.Position = new Point(500, 100);
    options.VSync = Settings.Instance.VSync ? VSyncMode.On : VSyncMode.Off;
    options.PreferredDepthBufferBits = 24;

#if VULKAN
    AutomataWindow.Instance.CreateWindow(options, ContextAPI.Vulkan);
#else
    AutomataWindow.Instance.CreateWindow(options, ContextAPI.OpenGL);
#endif

    AutomataWindow.Instance.Closing += ApplicationCloseCallbackImpl;
}

static void InitializeWorldImpl(out World world)
{
    world = new VoxelWorld(true);
    world.SystemManager.RegisterSystem<InputSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<RenderSystem, LastOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<AllocatedMeshingSystem<uint, PackedVertex>, RenderSystem>(SystemRegistrationOrder.Before);

    AllocatedMeshingSystem<uint, PackedVertex> allocatedMeshingSystem = world.SystemManager.GetSystem<AllocatedMeshingSystem<uint, PackedVertex>>();

    allocatedMeshingSystem.AllocateVertexAttributes(true, true,

        // vert
        new VertexAttribute<int>(0u, 1u, 0u, 0u),

        // uv
        new VertexAttribute<int>(1u, 1u, 4u, 0u),

        // model
        new VertexAttribute<float>(2u + 0u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M11)), 1u),
        new VertexAttribute<float>(2u + 1u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M21)), 1u),
        new VertexAttribute<float>(2u + 2u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M31)), 1u),
        new VertexAttribute<float>(2u + 3u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M41)), 1u)
    );

    allocatedMeshingSystem.SetTexture("Blocks", TextureAtlas.Instance.Blocks!);

    world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<ChunkModificationsSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<ChunkGenerationSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    World.RegisterWorld("Overworld", world);
}

static void InitializePlayerImpl(EntityManager entityManager)
{
    entityManager.CreateEntity(
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
        });
}

#endregion
