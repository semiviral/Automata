using System;
using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan.NativeExtensions
{
    public sealed class SurfaceExtension : KhrSurface
    {
        public SurfaceExtension(INativeContext nativeContext) : base(nativeContext) { }

        public unsafe SurfaceFormatKHR[] GetPhysicalDeviceSurfaceFormats(VulkanPhysicalDevice physicalDevice, SurfaceKHR surface)
        {
            uint formatsCount = 0u;
            GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formatsCount, (SurfaceFormatKHR*)null!);

            if (formatsCount is 0u)
            {
                return Array.Empty<SurfaceFormatKHR>();
            }

            SurfaceFormatKHR* surfaceFormatsPointer = stackalloc SurfaceFormatKHR[(int)formatsCount];
            GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formatsCount, surfaceFormatsPointer);
            SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[formatsCount];
            new Span<SurfaceFormatKHR>(surfaceFormatsPointer, (int)formatsCount).CopyTo(surfaceFormats);
            return surfaceFormats;
        }

        public unsafe PresentModeKHR[] GetPhysicalDeviceSurfacePresentModes(VulkanPhysicalDevice physicalDevice, SurfaceKHR surface)
        {
            uint presentationCount = 0u;
            GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, &presentationCount, (PresentModeKHR*)null!);

            if (presentationCount is 0u)
            {
                return Array.Empty<PresentModeKHR>();
            }

            PresentModeKHR* presentModesPointer = stackalloc PresentModeKHR[(int)presentationCount];
            GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, &presentationCount, presentModesPointer);
            PresentModeKHR[] presentModes = new PresentModeKHR[presentationCount];
            new Span<PresentModeKHR>(presentModesPointer, (int)presentationCount).CopyTo(presentModes);
            return presentModes;
        }
    }
}
