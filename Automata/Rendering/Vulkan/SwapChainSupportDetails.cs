#region

using Silk.NET.Vulkan;

#endregion

namespace Automata.Rendering.Vulkan
{
    public struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR SurfaceCapabilities { get; set; }
        public SurfaceFormatKHR[] Formats { get; set; }
        public PresentModeKHR[] PresentModes { get; set; }
    }
}
