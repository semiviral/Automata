using System;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public record VulkanExtension(string ExtensionName, uint Version);

    public class VulkanPhysicalDevice : VulkanObject
    {
        private readonly PhysicalDevice _PhysicalDevice;
        private readonly Memory<VulkanExtension> _Extensions;

        public PhysicalDeviceType Type { get; }
        public string Name { get; }

        public ReadOnlyMemory<VulkanExtension> Extensions => _Extensions;

        internal unsafe VulkanPhysicalDevice(Vk vk, PhysicalDevice physicalDevice) : base(vk)
        {
            _PhysicalDevice = physicalDevice;
            Handle = _PhysicalDevice.Handle;
            vk.GetPhysicalDeviceProperties(_PhysicalDevice, out PhysicalDeviceProperties properties);

            Type = properties.DeviceType;
            Name = SilkMarshal.PtrToString((nint)properties.DeviceName);

            uint extensionCount = 0u;
            VK.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null!, &extensionCount, (ExtensionProperties*)null!);
            Span<ExtensionProperties> extensionPropertiesSpan = stackalloc ExtensionProperties[(int)extensionCount];
            VK.EnumerateDeviceExtensionProperties(physicalDevice, string.Empty, &extensionCount, extensionPropertiesSpan);
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

        #region Conversions

        public static implicit operator PhysicalDevice(VulkanPhysicalDevice physicalDevice) => physicalDevice._PhysicalDevice;

        #endregion
    }
}
