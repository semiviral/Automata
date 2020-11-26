using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Concurrency;
using Automata.Engine.Input;
using Automata.Engine.Rendering;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.Vulkan;
using Automata.Game.Blocks;
using Automata.Game.Chunks;
using Automata.Game.Chunks.Generation;
using Automata.Game.Chunks.Generation.Meshing;
using Serilog;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Automata.Game
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Startup();
            await AutomataWindow.Instance.RunAsync();
            AutomataWindow.Instance.Dispose();
        }


        #region Main

        private static void Startup()
        {
            Settings.Load();
            InitializeLoggerAndValidateFiles();
            InitializeBoundedPool();
            InitializeWindow();

#if VULKAN
            InitializeVulkan();
#else
            BlockRegistry.Instance.LazyInitialize();
            InitializeWorld(out World world);
            InitializePlayer(world.EntityManager);
#endif
        }

        private static void ApplicationCloseCallback(object sender)
        {
            World.DisposeWorlds();
            ProgramRegistry.Instance.Dispose();
            TextureAtlas.Instance.Blocks?.Dispose();
            BoundedInvocationPool.Instance.Cancel();
        }

        #endregion


        #region Initialization

        private static void InitializeLoggerAndValidateFiles()
        {
            string specialPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string rootPath = Path.Combine(specialPath, "Automata/");
            Directory.CreateDirectory(rootPath);
            Directory.CreateDirectory(Path.Combine(rootPath, "Worlds"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Async(config => config.File(Path.Combine(rootPath, "Today.log")))
                .CreateLogger();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(Serilog), "Logger initialized."));
        }

        private static void InitializeBoundedPool()
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

        private static void InitializeWindow()
        {
            WindowOptions options = WindowOptions.Default;
            options.Title = "Automata";
            options.Size = new Size(800, 600);
            options.Position = new Point(500, 100);
            options.VSync = Settings.Instance.VSync;
            options.PreferredDepthBufferBits = 24;

#if VULKAN
            AutomataWindow.Instance.CreateWindow(options, ContextAPI.Vulkan);
#else
            AutomataWindow.Instance.CreateWindow(options, ContextAPI.OpenGL);
#endif

            AutomataWindow.Instance.Closing += ApplicationCloseCallback;
        }

        private static void InitializeWorld(out World world)
        {
            world = new VoxelWorld(true);
            world.SystemManager.RegisterBefore<InputSystem, FirstOrderSystem>();
            world.SystemManager.RegisterAfter<RenderSystem, LastOrderSystem>();
            world.SystemManager.RegisterBefore<TransformMatrixSystem, LastOrderSystem>();
            world.SystemManager.RegisterBefore<AllocatedMeshingSystem<uint, PackedVertex>, LastOrderSystem>();

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

            world.SystemManager.RegisterBefore<ChunkRegionSystem, DefaultOrderSystem>();
            world.SystemManager.RegisterAfter<ChunkModificationsSystem, ChunkRegionSystem>();
            world.SystemManager.RegisterAfter<ChunkGenerationSystem, ChunkModificationsSystem>();
            World.RegisterWorld("Overworld", world);
        }

        private static void InitializePlayer(EntityManager entityManager)
        {
            entityManager.CreateEntity(
                new Transform(),
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


        #region Vulkan

        private static void InitializeVulkan()
        {
            VulkanInstance instance = new VulkanInstance(VKAPI.Instance.VK, AutomataWindow.Instance.GetSurface(),
                new VulkanInstanceInfo("Automata.Game", new Version32(0u, 1u, 0u), "Automata.Engine", new Version32(0u, 1u, 0u), Vk.Version12),
                VKAPI.DebugInstanceExtensions, VKAPI.ValidationLayers);

            VulkanDebugMessenger debugMessenger = new VulkanDebugMessenger(instance);

            VulkanPhysicalDevice[] physicalDevices = instance.GetPhysicalDevices(IsPhysicalDeviceSuitable);
            VulkanPhysicalDevice physicalDevice = physicalDevices[0];
            VulkanLogicalDevice logicalDevice = physicalDevice.CreateLogicalDevice(VKAPI.LogicalDeviceExtensions, VKAPI.ValidationLayers);
        }

        private static bool IsPhysicalDeviceSuitable(VulkanPhysicalDevice physicalDevice)
        {
            if (physicalDevice.Type is not PhysicalDeviceType.DiscreteGpu || !physicalDevice.SupportsExtenstion(KhrSwapchain.ExtensionName))
            {
                return false;
            }

            SwapChainSupportDetails supportDetails = physicalDevice.SwapChainSupportDetails;

            if (supportDetails.Formats.Length is 0 || supportDetails.PresentModes.Length is 0)
            {
                return false;
            }

            QueueFamilyIndices queueFamilyIndices = physicalDevice.GetQueueFamilies();

            if (!queueFamilyIndices.IsCompleted())
            {
                return false;
            }

            PhysicalDeviceFeatures physicalDeviceFeatures = physicalDevice.GetFeatures();

            if (!physicalDeviceFeatures.GeometryShader)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
