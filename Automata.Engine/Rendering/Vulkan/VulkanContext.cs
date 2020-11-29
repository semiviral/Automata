namespace Automata.Engine.Rendering.Vulkan
{
    public record VulkanContext
    {
        public VulkanInstance? Instance { get; init; }
        public VulkanPhysicalDevice? PhysicalDevice { get; init; }
        public VulkanLogicalDevice? LogicalDevice { get; init; }
    }
}
