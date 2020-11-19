using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public abstract class VulkanObject
    {
        protected Vk VK { get; }

        public VulkanObject(Vk vk) => VK = vk;
    }
}
