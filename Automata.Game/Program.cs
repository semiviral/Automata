using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Automata.Engine;
using Automata.Engine.Components;
using Automata.Engine.Concurrency;
using Automata.Engine.Entities;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Systems;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Serilog;
using Silk.NET.OpenGL;
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

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Serilog), "Logger initialized."));

            Initialize();

            BufferObject<DrawElementsIndirectCommand> commands = new BufferObject<DrawElementsIndirectCommand>(GLAPI.Instance.GL);
            DrawElementsIndirectCommand command = new DrawElementsIndirectCommand
            {
                VertexCount = 0u,
                InstanceCount = 1u,
                AbsoluteVertexOffset = 6u,
                RelativeVertexOffset = 0u,
                BaseInstance = 0u,
            };
            commands.SetBufferData(1u, (uint)sizeof(DrawElementsIndirectCommand), (void*)&command, BufferDraw.DynamicDraw);

            BufferObject<byte> bufferObject = new BufferObject<byte>(GLAPI.Instance.GL);
            VertexArrayObject<byte> vao = new VertexArrayObject<byte>(GLAPI.Instance.GL, bufferObject, sizeof(uint) * 6, bufferObject);
            vao.AllocateVertexAttribute(new VertexAttribute<uint>(2u, 1u, (uint)sizeof(DrawElementsIndirectCommand), 1u));
            vao.AllocateVertexAttributes(new IVertexAttribute[]
            {
                new VertexAttribute<float>(3u + 0u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M11)), 1u),
                new VertexAttribute<float>(3u + 1u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M21)), 1u),
                new VertexAttribute<float>(3u + 2u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M31)), 1u),
                new VertexAttribute<float>(3u + 3u, 4u, (uint)Marshal.OffsetOf<Matrix4x4>(nameof(Matrix4x4.M41)), 1u),
            });

            AutomataWindow.Instance.Run();
        }

        private static void Initialize()
        {
            Settings.Load();

            if (Settings.Instance.SingleThreadedGeneration) BoundedInvocationPool.Instance.ModifyPoolSize(1);
            else BoundedInvocationPool.Instance.DefaultPoolSize();

            BoundedInvocationPool.Instance.ExceptionOccurred += (_, exception) => Log.Error($"{exception.Message}\r\n{exception.StackTrace}");

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
            BoundedInvocationPool.Instance.Cancel();
            GLAPI.Instance.GL.Dispose();
        }
    }
}
