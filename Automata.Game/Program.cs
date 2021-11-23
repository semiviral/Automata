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
using DiagnosticsProviderNS;
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

        private static void Startup()
        {
#if !FINAL_RELEASE
            DiagnosticsProvider.Enabled = true;
#endif

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


        #region Initialization

        private static void InitializeLoggerAndValidateFiles()
        {
            string special_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string root_path = Path.Combine(special_path, "Automata/");
            Directory.CreateDirectory(root_path);
            Directory.CreateDirectory(Path.Combine(root_path, "Worlds"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Async(config => config.File(Path.Combine(root_path, "Today.log")))
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

            AllocatedMeshingSystem<uint, PackedVertex> allocated_meshing_system = world.SystemManager.GetSystem<AllocatedMeshingSystem<uint, PackedVertex>>();

            allocated_meshing_system.AllocateVertexAttributes(true, true,

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

            allocated_meshing_system.SetTexture("Blocks", TextureAtlas.Instance.Blocks!);

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

            VulkanDebugMessenger debug_messenger = new VulkanDebugMessenger(instance);

            VulkanPhysicalDevice[] physical_devices = instance.GetPhysicalDevices(IsPhysicalDeviceSuitable);
            VulkanPhysicalDevice physical_device = physical_devices[0];
            VulkanLogicalDevice logical_device = physical_device.CreateLogicalDevice(VKAPI.LogicalDeviceExtensions, VKAPI.ValidationLayers);
            VulkanSwapChain swap_chain = logical_device.CreateSwapChain(ChooseSwapSurfaceFormat, ChooseSwapPresentationMode, ChooseSwapExtents);
        }

        private static bool IsPhysicalDeviceSuitable(VulkanPhysicalDevice physicalDevice)
        {
            if (physicalDevice.Type is not PhysicalDeviceType.DiscreteGpu || !physicalDevice.SupportsExtenstion(KhrSwapchain.ExtensionName))
            {
                return false;
            }

            SwapChainSupportDetails support_details = physicalDevice.SwapChainSupportDetails;

            if (support_details.Formats.Length is 0 || support_details.PresentModes.Length is 0)
            {
                return false;
            }

            QueueFamilyIndices queue_family_indices = physicalDevice.GetQueueFamilies();

            if (!queue_family_indices.IsCompleted())
            {
                return false;
            }

            PhysicalDeviceFeatures physical_device_features = physicalDevice.GetFeatures();

            if (!physical_device_features.GeometryShader)
            {
                return false;
            }

            return true;
        }

        private static SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] availableFormats)
        {
            foreach (SurfaceFormatKHR surface_format in availableFormats)
            {
                if ((surface_format.Format == Format.B8G8R8Srgb) && (surface_format.ColorSpace == ColorSpaceKHR.ColorspaceSrgbNonlinearKhr))
                {
                    return surface_format;
                }
            }

            return availableFormats[0];
        }

        private static PresentModeKHR ChooseSwapPresentationMode(PresentModeKHR[] availablePresentationModes)
        {
            foreach (PresentModeKHR presentation_mode in availablePresentationModes)
            {
                if (presentation_mode == PresentModeKHR.PresentModeMailboxKhr)
                {
                    return presentation_mode;
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
                Extent2D adjusted_extent = new Extent2D((uint)AutomataWindow.Instance.Size.X, (uint)AutomataWindow.Instance.Size.Y);

                adjusted_extent.Width = Math.Max(surfaceCapabilities.MinImageExtent.Width,
                    Math.Min(surfaceCapabilities.MinImageExtent.Width, adjusted_extent.Width));

                adjusted_extent.Height = Math.Max(surfaceCapabilities.MinImageExtent.Height,
                    Math.Min(surfaceCapabilities.MinImageExtent.Height, adjusted_extent.Height));

                return adjusted_extent;
            }
        }

        #endregion
    }
}
