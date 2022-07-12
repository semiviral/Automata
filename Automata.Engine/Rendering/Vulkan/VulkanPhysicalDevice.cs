using System;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public record VulkanExtension(string ExtensionName, uint Version);

    public class VulkanPhysicalDevice : VulkanObject
    {
        private readonly VulkanContext _Context;
        private readonly PhysicalDevice _PhysicalDevice;
        private readonly Memory<VulkanExtension> _Extensions;

        public uint APIVersion { get; }
        public uint DriverVersion { get; }
        public uint VendorID { get; }
        public uint DeviceID { get; }
        public PhysicalDeviceType Type { get; }
        public string Name { get; }
        public SwapChainSupportDetails SwapChainSupportDetails { get; }

        public override nint Handle => _PhysicalDevice.Handle;
        public ReadOnlyMemory<VulkanExtension> Extensions => _Extensions;

        internal unsafe VulkanPhysicalDevice(Vk vk, VulkanContext context, PhysicalDevice physicalDevice) : base(vk)
        {
            if (context.Instance is null)
            {
                throw new NullReferenceException(nameof(context.Instance));
            }

            _Context = context;
            _PhysicalDevice = physicalDevice;
            VK.GetPhysicalDeviceProperties(this, out PhysicalDeviceProperties properties);

            APIVersion = properties.ApiVersion;
            DriverVersion = properties.DriverVersion;
            VendorID = properties.VendorID;
            DeviceID = properties.DeviceID;
            Type = properties.DeviceType;
            Name = SilkMarshal.PtrToString((nint)properties.DeviceName);

            SwapChainSupportDetails = GetSwapChainSupport();
            _Extensions = GetExtensions();
        }

        private SwapChainSupportDetails GetSwapChainSupport()
        {
            _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfaceCapabilities(this, _Context.Instance!.Surface,
                out SurfaceCapabilitiesKHR surface_capabilities);

            return new SwapChainSupportDetails
            {
                SurfaceCapabilities = surface_capabilities,
                Formats = _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfaceFormats(this, _Context.Instance!.Surface),
                PresentModes = _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfacePresentModes(this, _Context.Instance!.Surface)
            };
        }

        private unsafe Memory<VulkanExtension> GetExtensions()
        {
            uint extension_count = 0u;
            VK.EnumerateDeviceExtensionProperties(this, (byte*)null!, &extension_count, (ExtensionProperties*)null!);
            Span<ExtensionProperties> extension_properties_span = stackalloc ExtensionProperties[(int)extension_count];
            VK.EnumerateDeviceExtensionProperties(this, string.Empty, &extension_count, extension_properties_span);
            Memory<VulkanExtension> extensions = new VulkanExtension[extension_count];
            Span<VulkanExtension> extensions_span = extensions.Span;

            for (int index = 0; index < extensions_span.Length; index++)
            {
                ExtensionProperties extension_properties = extension_properties_span[index];
                string name = SilkMarshal.PtrToString((nint)extension_properties.ExtensionName);
                extensions_span[index] = new VulkanExtension(name, extension_properties.SpecVersion);
            }

            return extensions;
        }

        public bool SupportsExtenstion(string extensionName)
        {
            foreach (VulkanExtension extension in _Extensions.Span)
            {
                if (extension.ExtensionName.Equals(extensionName))
                {
                    return true;
                }
            }

            return false;
        }

        public PhysicalDeviceFeatures GetFeatures()
        {
            VK.GetPhysicalDeviceFeatures(this, out PhysicalDeviceFeatures physical_device_features);
            return physical_device_features;
        }

        public unsafe QueueFamilyIndices GetQueueFamilies()
        {
            // todo make this choose with a dynamic predicate

            QueueFamilyIndices queue_family_indices = new QueueFamilyIndices();

            uint queue_family_properties_count = 0u;
            VK.GetPhysicalDeviceQueueFamilyProperties(this, &queue_family_properties_count, (QueueFamilyProperties*)null!);
            QueueFamilyProperties* queue_family_properties_pointer = stackalloc QueueFamilyProperties[(int)queue_family_properties_count];
            VK.GetPhysicalDeviceQueueFamilyProperties(this, &queue_family_properties_count, queue_family_properties_pointer);

            for (uint index = 0; index < queue_family_properties_count; index++)
            {
                QueueFamilyProperties queue_family_properties = queue_family_properties_pointer[index];

                if (queue_family_properties.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    queue_family_indices.GraphicsFamily = index;
                }

                _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfaceSupport(this, index, _Context.Instance!.Surface, out Bool32 presentation_support);

                if (presentation_support)
                {
                    queue_family_indices.PresentationFamily = index;
                }

                if (queue_family_indices.IsCompleted())
                {
                    break;
                }
            }

            return queue_family_indices;
        }

        public VulkanLogicalDevice CreateLogicalDevice(string[] extensions, string[]? validationLayers) =>
            new VulkanLogicalDevice(VK, _Context with { PhysicalDevice = this }, extensions, validationLayers);


        #region Conversions

        public static implicit operator PhysicalDevice(VulkanPhysicalDevice physicalDevice) => physicalDevice._PhysicalDevice;

        #endregion
    }
}
