using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Extensions;
using Automata.Engine.Rendering.Vulkan.NativeExtensions;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public sealed record VulkanInstanceInfo(string ApplicationName, Version32 ApplicationVersion, string EngineName, Version32 EngineVersion,
        Version32 APIVersion);

    public sealed class VulkanInstance : IDisposable
    {
        private readonly Instance _VKInstance;

        internal Vk VK { get; }

        public VulkanInstanceInfo Info { get; }
        public SurfaceExtension SurfaceExtension { get; }
        public SurfaceKHR Surface { get; }
        public VkHandle Handle { get; }

        public unsafe VulkanInstance(Vk vk, IVkSurface vkSurface, VulkanInstanceInfo info, string[] requestedExtensions, string[]? validationLayers)
        {
            VK = vk;
            Info = info;

            CreateInstance(vkSurface, requestedExtensions, validationLayers, out _VKInstance);
            SurfaceExtension = GetInstanceExtension<SurfaceExtension>();
            Handle = _VKInstance.ToHandle();
            Surface = vkSurface.Create(Handle, (AllocationCallbacks*)null!).ToSurface();

            Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "Created."));
        }

        private unsafe void CreateInstance(IVkSurface vkSurface, string[] requestedExtensions, string[]? validationLayers, out Instance instance)
        {
            nint application_name = Info.ApplicationName.Marshal();
            nint engine_name = Info.EngineName.Marshal();

            ApplicationInfo application_info = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)application_name,
                ApplicationVersion = Info.ApplicationVersion,
                PEngineName = (byte*)engine_name,
                EngineVersion = Info.EngineVersion,
                ApiVersion = Info.APIVersion
            };

            string[] required_extensions = VKAPI.GetRequiredExtensions(vkSurface, requestedExtensions);
            nint required_extensions_pointer = SilkMarshal.StringArrayToPtr(required_extensions);
            nint enabled_layer_names = 0x0;
            int enabled_layer_count = 0;

            InstanceCreateInfo create_info = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &application_info,
                PpEnabledExtensionNames = (byte**)required_extensions_pointer,
                EnabledExtensionCount = (uint)required_extensions.Length,
                PpEnabledLayerNames = (byte**)null!,
                EnabledLayerCount = 0,
                PNext = (void*)null!
            };

            if (validationLayers is not null)
            {
                VKAPI.VerifyValidationLayerSupport(VK, validationLayers);

                enabled_layer_names = SilkMarshal.StringArrayToPtr(validationLayers);
                enabled_layer_count = validationLayers.Length;
                create_info.PpEnabledLayerNames = (byte**)enabled_layer_names;
                create_info.EnabledLayerCount = (uint)enabled_layer_count;
            }

            Log.Information(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(VulkanInstance), "allocating instance."));
            Result result = VK.CreateInstance(&create_info, (AllocationCallbacks*)null!, out instance);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create Vulkan instance.");
            }

            SilkMarshal.FreeString(application_name);
            SilkMarshal.FreeString(engine_name);
            SilkMarshal.FreeString(required_extensions_pointer);

            if (enabled_layer_names is not 0x0 && enabled_layer_count is not 0)
            {
                SilkMarshal.FreeString(enabled_layer_names);
            }
        }

        public unsafe VulkanPhysicalDevice[] GetPhysicalDevices(Predicate<VulkanPhysicalDevice>? suitability)
        {
            suitability ??= _ => true;

            uint device_count = 0u;
            VK.EnumeratePhysicalDevices(_VKInstance, &device_count, Span<PhysicalDevice>.Empty);
            Span<PhysicalDevice> physical_devices = stackalloc PhysicalDevice[(int)device_count];
            VK.EnumeratePhysicalDevices(_VKInstance, &device_count, physical_devices);
            VulkanPhysicalDevice[] temp_suitable = ArrayPool<VulkanPhysicalDevice>.Shared.Rent((int)device_count);
            int index = 0;

            foreach (PhysicalDevice physical_device in physical_devices)
            {
                VulkanPhysicalDevice vulkan_physical_device = new VulkanPhysicalDevice(VK, new VulkanContext
                {
                    Instance = this
                }, physical_device);

                if (suitability(vulkan_physical_device))
                {
                    temp_suitable[index] = vulkan_physical_device;
                    index += 1;
                }
            }

            VulkanPhysicalDevice[] final_suitable = temp_suitable[..index];
            ArrayPool<VulkanPhysicalDevice>.Shared.Return(temp_suitable);
            return final_suitable;
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

        #endregion


        #region Conversions

        public static implicit operator Instance(VulkanInstance vulkanInstance) => vulkanInstance._VKInstance;

        #endregion


        #region IDisposable

        public unsafe void Dispose() { VK.DestroyInstance(_VKInstance, (AllocationCallbacks*)null!); }

        #endregion
    }
}
