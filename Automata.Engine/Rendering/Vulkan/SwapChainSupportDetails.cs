using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR SurfaceCapabilities { get; set; }
        public SurfaceFormatKHR[] Formats { get; set; }
        public PresentModeKHR[] PresentModes { get; set; }
    }
}
