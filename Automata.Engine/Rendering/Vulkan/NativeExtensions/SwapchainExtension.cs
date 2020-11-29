using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan.NativeExtensions
{
    public sealed class SwapchainExtension : KhrSwapchain
    {
        public SwapchainExtension(INativeContext nativeContext) : base(nativeContext) { }
    }
}
