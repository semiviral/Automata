using System.Runtime.InteropServices;
using Automata.Engine.Extensions;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanInstance : VulkanObject
    {
        private static readonly string _LogFormat = string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "{0}");

        private readonly Instance _VKInstance;

        public string ApplicationName { get; }
        public Version32 ApplicationVersion { get; }
        public string EngineName { get; }
        public Version32 EngineVersion { get; }
        public Version32 APIVersion { get; }
        public KhrSwapchain Swapchain { get; }
        public KhrSurface Surface { get; }

        public VulkanInstance(Vk vk, string applicationName, Version32 applicationVersion, string engineName, Version32 engineVersion, Version32 apiVersion,
            string[] requestedExtensions, bool validation, string[] validationLayers) : base(vk)
        {
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            EngineName = engineName;
            EngineVersion = engineVersion;
            APIVersion = apiVersion;

            _VKInstance = Create(requestedExtensions, validation, validationLayers);

            if (!TryGetInstanceExtension(out KhrSwapchain? swapchain))
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, "KHR_swapchain extension not found.");
            }
            else
            {
                Swapchain = swapchain!;
            }

            if (!TryGetInstanceExtension(out KhrSurface? surface))
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, "KHR_surface extension not found.");
            }
            else
            {
                Surface = surface!;
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
                VKAPI.Validate(VK, ref instanceCreateInfo, validationLayers);
            }

            Log.Information(string.Format(_LogFormat, "allocating instance."));
            Result result;

            if ((result = VK.CreateInstance(ref instanceCreateInfo, ref VKAPI.AllocationCallback, out Instance instance)) != Result.Success)
            {
                throw new VulkanException(result, "Failed to create Vulkan instance.");
            }

            Marshal.FreeHGlobal(applicationName);
            Marshal.FreeHGlobal(engineName);
            SilkMarshal.FreeStringArrayPtr(requiredExtensionsPointer, requestedExtensions.Length);

            return instance;
        }

        public bool TryGetInstanceExtension<T>(out T? extenstion) where T : NativeExtension<Vk> => VK.TryGetInstanceExtension(_VKInstance, out extenstion);

        public bool TryGetDeviceExtension<T>(out T? extension, Device device) where T : NativeExtension<Vk> =>
            VK.TryGetDeviceExtension(_VKInstance, device, out extension);
    }
}
