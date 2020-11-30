using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Concurrency;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
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
using Vector = Automata.Engine.Numerics.Vector;

namespace Automata.Game
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Vector2<int> a = new Vector2<int>(-1);
            Vector2<int> b = Vector2<int>.Zero;
            Vector2<bool> result = a < b;

            bool final = Vector.All(result);

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
            TextureAtlas.Instance.Dispose();
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
                new ChunkLoader
                {
                    Radius = 5
                });

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
            VulkanSwapChain swapChain = logicalDevice.CreateSwapChain(ChooseSwapSurfaceFormat, ChooseSwapPresentationMode, ChooseSwapExtents);
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

        private static SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] availableFormats)
        {
            foreach (SurfaceFormatKHR surfaceFormat in availableFormats)
            {
                if ((surfaceFormat.Format == Format.B8G8R8Srgb) && (surfaceFormat.ColorSpace == ColorSpaceKHR.ColorspaceSrgbNonlinearKhr))
                {
                    return surfaceFormat;
                }
            }

            return availableFormats[0];
        }

        private static PresentModeKHR ChooseSwapPresentationMode(PresentModeKHR[] availablePresentationModes)
        {
            foreach (PresentModeKHR presentationMode in availablePresentationModes)
            {
                if (presentationMode == PresentModeKHR.PresentModeMailboxKhr)
                {
                    return presentationMode;
                }
            }

            return PresentModeKHR.PresentModeFifoKhr;
        }

        private static Extent2D ChooseSwapExtents(SurfaceCapabilitiesKHR surfaceCapabilities)
        {
            if (surfaceCapabilities.CurrentExtent.Width != int.MaxValue)
            {
                return surfaceCapabilities.CurrentExtent;
            }
            else
            {
                Extent2D adjustedExtent = new Extent2D((uint)AutomataWindow.Instance.Size.X, (uint)AutomataWindow.Instance.Size.Y);

                adjustedExtent.Width = Math.Max(surfaceCapabilities.MinImageExtent.Width,
                    Math.Min(surfaceCapabilities.MinImageExtent.Width, adjustedExtent.Width));

                adjustedExtent.Height = Math.Max(surfaceCapabilities.MinImageExtent.Height,
                    Math.Min(surfaceCapabilities.MinImageExtent.Height, adjustedExtent.Height));

                return adjustedExtent;
            }
        }

        #endregion
    }
}
