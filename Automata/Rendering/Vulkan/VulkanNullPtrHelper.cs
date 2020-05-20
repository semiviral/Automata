#region

using Silk.NET.Vulkan;

#endregion

namespace Automata.Rendering.Vulkan
{
    public unsafe class VulkanNullPtrHelper
    {
        public static readonly AllocationCallbacks* AllocationCallbacks = null;
        public static readonly PhysicalDevice* PhysicalDevice = null;
        public static readonly ExtensionProperties* ExtensionProperties = null;
        public static readonly QueueFamilyProperties* QueueFamilyProperties = null;
    }
}
