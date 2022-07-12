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
            uint formats_count = 0u;
            GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formats_count, (SurfaceFormatKHR*)null!);

            if (formats_count is 0u)
            {
                return Array.Empty<SurfaceFormatKHR>();
            }

            SurfaceFormatKHR* surface_formats_pointer = stackalloc SurfaceFormatKHR[(int)formats_count];
            GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, &formats_count, surface_formats_pointer);
            SurfaceFormatKHR[] surface_formats = new SurfaceFormatKHR[formats_count];
            new Span<SurfaceFormatKHR>(surface_formats_pointer, (int)formats_count).CopyTo(surface_formats);
            return surface_formats;
        }

        public unsafe PresentModeKHR[] GetPhysicalDeviceSurfacePresentModes(VulkanPhysicalDevice physicalDevice, SurfaceKHR surface)
        {
            uint presentation_count = 0u;
            GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, &presentation_count, (PresentModeKHR*)null!);

            if (presentation_count is 0u)
            {
                return Array.Empty<PresentModeKHR>();
            }

            PresentModeKHR* present_modes_pointer = stackalloc PresentModeKHR[(int)presentation_count];
            GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, &presentation_count, present_modes_pointer);
            PresentModeKHR[] present_modes = new PresentModeKHR[presentation_count];
            new Span<PresentModeKHR>(present_modes_pointer, (int)presentation_count).CopyTo(present_modes);
            return present_modes;
        }
    }
}
