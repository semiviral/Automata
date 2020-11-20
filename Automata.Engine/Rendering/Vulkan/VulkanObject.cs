using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public abstract class VulkanObject
    {
        protected readonly Vk VK;

        public nuint Handle { get; protected init; }

        protected VulkanObject(Vk vk) => VK = vk;
    }
}
