using System;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Extensions;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public sealed record VulkanInstanceInfo(string ApplicationName, Version32 ApplicationVersion, string EngineName, Version32 EngineVersion,
        Version32 APIVersion);

    public record VulkanInstance : IDisposable
    {
        private readonly Instance _VKInstance;

        internal Vk VK { get; }
        private VulkanInstanceInfo Info { get; }
        public SurfaceExtension SurfaceExtension { get; }
        public VkHandle Handle { get; }

        public VulkanInstance(Vk vk, IVkSurface vkSurface, VulkanInstanceInfo info, string[] requestedExtensions, bool validation,
            string[] validationLayers)
        {
            VK = vk;
            Info = info;

            CreateInstance(vkSurface, requestedExtensions, validation, validationLayers, out _VKInstance);
            SurfaceExtension = GetInstanceExtension<SurfaceExtension>();
            Handle = _VKInstance.ToHandle();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "Created."));
        }

        private unsafe void CreateInstance(IVkSurface vkSurface, string[] requestedExtensions, bool validation, string[] validationLayers, out Instance instance)
        {
            nint applicationName = Info.ApplicationName.Marshal();
            nint engineName = Info.EngineName.Marshal();

            ApplicationInfo applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)applicationName,
                ApplicationVersion = Info.ApplicationVersion,
                PEngineName = (byte*)engineName,
                EngineVersion = Info.EngineVersion,
                ApiVersion = Info.APIVersion
            };

            string[] requiredExtensions = VKAPI.GetRequiredExtensions(vkSurface, requestedExtensions);
            nint requiredExtensionsPointer = SilkMarshal.StringArrayToPtr(requiredExtensions);
            nint enabledLayerNames = 0x0;
            int enabledLayerCount = 0;

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = (byte**)requiredExtensionsPointer,
                EnabledExtensionCount = (uint)requiredExtensions.Length,
                PpEnabledLayerNames = (byte**)null!,
                EnabledLayerCount = 0,
                PNext = (void*)null!
            };

            if (validation)
            {
                VKAPI.VerifyValidationLayerSupport(VK, validationLayers);

                enabledLayerNames = SilkMarshal.StringArrayToPtr(validationLayers);
                enabledLayerCount = validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)enabledLayerNames;
                createInfo.EnabledLayerCount = (uint)enabledLayerCount;
            }

            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "allocating instance."));
            Result result = VK.CreateInstance(&createInfo, (AllocationCallbacks*)null!, out instance);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create Vulkan instance.");
            }

            SilkMarshal.FreeString(applicationName);
            SilkMarshal.FreeString(engineName);
            SilkMarshal.FreeString(requiredExtensionsPointer);

            if (enabledLayerNames is not 0x0 && enabledLayerCount is not 0)
            {
                SilkMarshal.FreeString(enabledLayerNames);
            }
        }

        public unsafe Span<PhysicalDevice> GetPhysicalDevices()
        {
            uint deviceCount = 0u;
            VK.EnumeratePhysicalDevices(_VKInstance, &deviceCount, Span<PhysicalDevice>.Empty);
            Span<PhysicalDevice> physicalDevices = new PhysicalDevice[deviceCount];
            VK.EnumeratePhysicalDevices(_VKInstance, (uint*)null!, physicalDevices);

            return physicalDevices;
        }

        #region Get Extension

        public TExtension GetInstanceExtension<TExtension>() where TExtension : NativeExtension<Vk>
        {
            if (VK.TryGetInstanceExtension(_VKInstance, out TExtension? extension))
            {
                return extension!;
            }
            else
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, $"{typeof(TExtension).Name} extension not present.");
            }
        }

        public bool TryGetInstanceExtension<T>([NotNullWhen(true)] out T? extenstion) where T : NativeExtension<Vk> =>
            VK.TryGetInstanceExtension(_VKInstance, out extenstion);

        public bool TryGetDeviceExtension<T>(Device device, [NotNullWhen(true)] out T? extension) where T : NativeExtension<Vk> =>
            VK.TryGetDeviceExtension(_VKInstance, device, out extension);

        #endregion


        #region Conversions

        public static explicit operator Instance(VulkanInstance vulkanInstance) => vulkanInstance._VKInstance;

        #endregion


        #region IDisposable

        public unsafe void Dispose()
        {
            VK.DestroyInstance(_VKInstance, (AllocationCallbacks*)null!);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
