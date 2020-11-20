using Silk.NET.Core.Contexts;

namespace Automata.Engine.Rendering.Vulkan.KHR
{
    public class Surface : VulkanObject
    {
        public unsafe Surface(VulkanInstance instance, IVkSurface vkSurface) : base(instance.VK) => vkSurface.Create(instance.Handle, (byte*)null!);
    }
}
