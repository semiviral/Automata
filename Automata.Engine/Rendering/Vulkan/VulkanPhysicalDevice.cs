using System;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public record VulkanExtension(string ExtensionName, uint Version);

    public class VulkanPhysicalDevice : VulkanObject
    {
        private readonly VulkanInstance _Instance;
        private readonly PhysicalDevice _PhysicalDevice;
        private readonly Memory<VulkanExtension> _Extensions;

        public uint APIVersion { get; }
        public uint DriverVersion { get; }
        public uint VendorID { get; }
        public uint DeviceID { get; }
        public PhysicalDeviceType Type { get; }
        public string Name { get; }

        public ReadOnlyMemory<VulkanExtension> Extensions => _Extensions;

        internal unsafe VulkanPhysicalDevice(Vk vk, VulkanInstance instance, PhysicalDevice physicalDevice) : base(vk)
        {
            _Instance = instance;
            _PhysicalDevice = physicalDevice;
            Handle = _PhysicalDevice.Handle;
            VK.GetPhysicalDeviceProperties(this, out PhysicalDeviceProperties properties);

            APIVersion = properties.ApiVersion;
            DriverVersion = properties.DriverVersion;
            VendorID = properties.VendorID;
            DeviceID = properties.DeviceID;
            Type = properties.DeviceType;
            Name = SilkMarshal.PtrToString((nint)properties.DeviceName);

            uint extensionCount = 0u;
            VK.EnumerateDeviceExtensionProperties(this, (byte*)null!, &extensionCount, (ExtensionProperties*)null!);
            Span<ExtensionProperties> extensionPropertiesSpan = stackalloc ExtensionProperties[(int)extensionCount];
            VK.EnumerateDeviceExtensionProperties(this, string.Empty, &extensionCount, extensionPropertiesSpan);
            _Extensions = new VulkanExtension[extensionCount];
            Span<VulkanExtension> extensions = _Extensions.Span;

            for (int index = 0; index < extensionCount; index++)
            {
                ExtensionProperties extensionProperties = extensionPropertiesSpan[index];
                string name = SilkMarshal.PtrToString((nint)extensionProperties.ExtensionName);
                extensions[index] = new VulkanExtension(name, extensionProperties.SpecVersion);
            }
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

        public SwapChainSupportDetails GetSwapChainSupport()
        {
            _Instance.SurfaceExtension.GetPhysicalDeviceSurfaceCapabilities(this, _Instance.Surface, out SurfaceCapabilitiesKHR surfaceCapabilities);

            return new SwapChainSupportDetails
            {
                SurfaceCapabilities = surfaceCapabilities,
                Formats = _Instance.SurfaceExtension.GetPhysicalDeviceSurfaceFormats(this, _Instance.Surface),
                PresentModes = _Instance.SurfaceExtension.GetPhysicalDeviceSurfacePresentModes(this, _Instance.Surface)
            };
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

                _Instance.SurfaceExtension.GetPhysicalDeviceSurfaceSupport(this, index, _Instance.Surface, out Bool32 presentationSupport);

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


        #region Conversions

        public static implicit operator PhysicalDevice(VulkanPhysicalDevice physicalDevice) => physicalDevice._PhysicalDevice;

        #endregion
    }
}
