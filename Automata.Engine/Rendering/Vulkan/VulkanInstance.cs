using System.Runtime.InteropServices;
using Automata.Engine.Extensions;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public record VulkanInstance
    {
        private static readonly string _LogFormat = string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "{0}");

        private readonly Instance _VKInstance;
        private readonly Vk _VK;

        public string ApplicationName { get; }
        public Version32 ApplicationVersion { get; }
        public string EngineName { get; }
        public Version32 EngineVersion { get; }
        public Version32 APIVersion { get; }
        public SwapchainExtension SwapchainExtension { get; }
        public SurfaceExtension SurfaceExtension { get; }
        public VkHandle Handle { get; }

        public VulkanInstance(Vk vk, string applicationName, Version32 applicationVersion, string engineName, Version32 engineVersion, Version32 apiVersion,
            string[] requestedExtensions, bool validation, string[] validationLayers)
        {
            _VK = vk;

            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            EngineName = engineName;
            EngineVersion = engineVersion;
            APIVersion = apiVersion;

            _VKInstance = Create(requestedExtensions, validation, validationLayers);
            Handle = _VKInstance.ToHandle();

            if (!TryGetInstanceExtension(out SwapchainExtension? swapchainExtension))
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, "KHR_swapchain extension not found.");
            }
            else
            {
                SwapchainExtension = swapchainExtension!;
            }

            if (!TryGetInstanceExtension(out SurfaceExtension? surfaceExtension))
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, $"{SurfaceExtension.EXTENSION_NAME} extension not found.");
            }
            else
            {
                SurfaceExtension = surfaceExtension!;
            }

            // todo extend this debug info
            Log.Debug(_LogFormat, "Created.");
        }

        private unsafe Instance Create(string[] requestedExtensions, bool validation, string[] validationLayers)
        {
            nint applicationName = ApplicationName.MarshalANSI();
            nint engineName = EngineName.MarshalANSI();

            ApplicationInfo applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)applicationName,
                ApplicationVersion = ApplicationVersion,
                PEngineName = (byte*)engineName,
                EngineVersion = EngineVersion,
                ApiVersion = APIVersion
            };

            string[] requiredExtensions = VKAPI.GetRequiredExtensions(requestedExtensions);
            nint requiredExtensionsPointer = SilkMarshal.MarshalStringArrayToPtr(requiredExtensions);

            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = (byte**)requiredExtensionsPointer,
                EnabledExtensionCount = (uint)requiredExtensions.Length,
                EnabledLayerCount = 0,
                PpEnabledLayerNames = (byte**)null!,
                PNext = (void*)null!
            };

            if (validation)
            {
                VKAPI.Validate(_VK, ref instanceCreateInfo, validationLayers);
            }

            Log.Information(string.Format(_LogFormat, "allocating instance."));
            Result result;

            if ((result = _VK.CreateInstance(ref instanceCreateInfo, ref VKAPI.AllocationCallback, out Instance instance)) != Result.Success)
            {
                throw new VulkanException(result, "Failed to create Vulkan instance.");
            }

            Marshal.FreeHGlobal(applicationName);
            Marshal.FreeHGlobal(engineName);
            SilkMarshal.FreeStringArrayPtr(requiredExtensionsPointer, requestedExtensions.Length);

            return instance;
        }

        public bool TryGetInstanceExtension<T>(out T? extenstion) where T : NativeExtension<Vk> => _VK.TryGetInstanceExtension(_VKInstance, out extenstion);

        public bool TryGetDeviceExtension<T>(out T? extension, Device device) where T : NativeExtension<Vk> =>
            _VK.TryGetDeviceExtension(_VKInstance, device, out extension);
    }
}
