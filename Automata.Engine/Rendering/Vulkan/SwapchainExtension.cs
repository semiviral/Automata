using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan
{
    public abstract class SwapchainExtension : KhrSwapchain
    {
        protected SwapchainExtension(INativeContext nativeContext) : base(nativeContext) { }
    }
}
