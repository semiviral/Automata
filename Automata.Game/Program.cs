using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Game;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Automata.Game.Chunks.Generation.Meshing;
using Serilog;
using Silk.NET.Windowing.Common;

Settings.Load();
InitializeLogger();
InitializeBoundedPool();
InitializeWindow();
BlockRegistry.Instance.LazyInitialize();
InitializeWorld(out World world);
InitializePlayer(world.EntityManager);
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
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .WriteTo.Async(config => config.File("log.txt"))
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

static unsafe void InitializeWorld(out World world)
{
    world = new VoxelWorld(true);
    world.SystemManager.RegisterSystem<InputSystem, FirstOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<RenderSystem, LastOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<AllocatedMeshingSystem<uint, PackedVertex>, RenderSystem>(SystemRegistrationOrder.Before);

    AllocatedMeshingSystem<uint, PackedVertex> allocatedMeshingSystem = world.SystemManager.GetSystem<AllocatedMeshingSystem<uint, PackedVertex>>();

    allocatedMeshingSystem.AllocateVertexAttributes(true, true,

        // vert
        new VertexAttribute<int>(0u, 1u, 0u),

        // uv
        new VertexAttribute<int>(1u, 1u, 4u),

        // drawID
        new VertexAttribute<uint>(2u, 1u, (uint)Marshal.OffsetOf<DrawElementsIndirectCommand>(nameof(DrawElementsIndirectCommand.BaseInstance)), 1u, 1u),

        // model
        new VertexAttribute<float>(3u + 0u, 4u, (uint)(sizeof(Vector4) * 0), 2u, 1u),
        new VertexAttribute<float>(3u + 1u, 4u, (uint)(sizeof(Vector4) * 1), 2u, 1u),
        new VertexAttribute<float>(3u + 2u, 4u, (uint)(sizeof(Vector4) * 2), 2u, 1u),
        new VertexAttribute<float>(3u + 3u, 4u, (uint)(sizeof(Vector4) * 3), 2u, 1u)
        );

    allocatedMeshingSystem.SetTexture("Blocks", TextureAtlas.Instance.Blocks!);

    // world.SystemManager.RegisterSystem<ChunkRegionLoaderSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<ChunkModificationsSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    world.SystemManager.RegisterSystem<ChunkGenerationSystem, DefaultOrderSystem>(SystemRegistrationOrder.Before);
    World.RegisterWorld("Overworld", world);
}

static void InitializePlayer(EntityManager entityManager)
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

    entityManager.CreateEntity(new Chunk
    {
        State = GenerationState.AwaitingTerrain
    }, new Translation
    {
        Value = new Vector3(0f, 96f, 0f)
    });

    // entityManager.CreateEntity(new Chunk
    // {
    //     State = GenerationState.AwaitingTerrain
    // }, new Translation
    // {
    //     Value = new Vector3(32f, 96f, 32f)
    // });
}

#endregion
