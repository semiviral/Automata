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
                out SurfaceCapabilitiesKHR surfaceCapabilities);

            return new SwapChainSupportDetails
            {
                SurfaceCapabilities = surfaceCapabilities,
                Formats = _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfaceFormats(this, _Context.Instance!.Surface),
                PresentModes = _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfacePresentModes(this, _Context.Instance!.Surface)
            };
        }

        private unsafe Memory<VulkanExtension> GetExtensions()
        {
            uint extensionCount = 0u;
            VK.EnumerateDeviceExtensionProperties(this, (byte*)null!, &extensionCount, (ExtensionProperties*)null!);
            Span<ExtensionProperties> extensionPropertiesSpan = stackalloc ExtensionProperties[(int)extensionCount];
            VK.EnumerateDeviceExtensionProperties(this, string.Empty, &extensionCount, extensionPropertiesSpan);
            Memory<VulkanExtension> extensions = new VulkanExtension[extensionCount];
            Span<VulkanExtension> extensionsSpan = extensions.Span;

            for (int index = 0; index < extensionsSpan.Length; index++)
            {
                ExtensionProperties extensionProperties = extensionPropertiesSpan[index];
                string name = SilkMarshal.PtrToString((nint)extensionProperties.ExtensionName);
                extensionsSpan[index] = new VulkanExtension(name, extensionProperties.SpecVersion);
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
            VK.GetPhysicalDeviceFeatures(this, out PhysicalDeviceFeatures physicalDeviceFeatures);
            return physicalDeviceFeatures;
        }

        public unsafe QueueFamilyIndices GetQueueFamilies()
        {
            // todo make this choose with a dynamic predicate

            QueueFamilyIndices queueFamilyIndices = new QueueFamilyIndices();

            uint queueFamilyPropertiesCount = 0u;
            VK.GetPhysicalDeviceQueueFamilyProperties(this, &queueFamilyPropertiesCount, (QueueFamilyProperties*)null!);
            QueueFamilyProperties* queueFamilyPropertiesPointer = stackalloc QueueFamilyProperties[(int)queueFamilyPropertiesCount];
            VK.GetPhysicalDeviceQueueFamilyProperties(this, &queueFamilyPropertiesCount, queueFamilyPropertiesPointer);

            for (uint index = 0; index < queueFamilyPropertiesCount; index++)
            {
                QueueFamilyProperties queueFamilyProperties = queueFamilyPropertiesPointer[index];

                if (queueFamilyProperties.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    queueFamilyIndices.GraphicsFamily = index;
                }

                _Context.Instance!.SurfaceExtension.GetPhysicalDeviceSurfaceSupport(this, index, _Context.Instance!.Surface, out Bool32 presentationSupport);

                if (presentationSupport)
                {
                    queueFamilyIndices.PresentationFamily = index;
                }

                if (queueFamilyIndices.IsCompleted())
                {
                    break;
                }
            }

            return queueFamilyIndices;
        }

        public VulkanLogicalDevice CreateLogicalDevice(string[] extensions, string[]? validationLayers) =>
            new VulkanLogicalDevice(VK, _Context with { PhysicalDevice = this }, extensions, validationLayers);


        #region Conversions

        public static implicit operator PhysicalDevice(VulkanPhysicalDevice physicalDevice) => physicalDevice._PhysicalDevice;

        #endregion
    }
}
