using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanSwapChain : VulkanObject
    {
        public delegate SurfaceFormatKHR ChooseSwapChainSurfaceFormat(SurfaceFormatKHR[] formats);

        public delegate PresentModeKHR ChooseSwapChainPresentMode(PresentModeKHR[] presentModes);

        public delegate Extent2D ChooseSwapChainExtents(SurfaceCapabilitiesKHR surfaceCapabilities);

        private readonly VulkanContext _Context;
        private readonly SwapchainKHR _SwapchainKHR;
        private readonly Image[] _Images;

        public SurfaceFormatKHR SurfaceFormat { get; }
        public PresentModeKHR PresentMode { get; }
        public Extent2D Extents { get; }

        internal unsafe VulkanSwapChain(Vk vk, VulkanContext context, ChooseSwapChainSurfaceFormat chooseFormat, ChooseSwapChainPresentMode choosePresentMode,
            ChooseSwapChainExtents chooseExtents) : base(vk)
        {
            static uint get_max_image_count_impl_impl(SurfaceCapabilitiesKHR surfaceCapabilities)
            {
                uint min_image_count = surfaceCapabilities.MinImageCount + 1;

                if ((surfaceCapabilities.MinImageCount > 0) && (min_image_count > surfaceCapabilities.MaxImageCount))
                {
                    min_image_count = surfaceCapabilities.MaxImageCount;
                }

                return min_image_count;
            }

            _Context = context;
            SurfaceFormat = chooseFormat(_Context.PhysicalDevice!.SwapChainSupportDetails.Formats);
            PresentMode = choosePresentMode(_Context.PhysicalDevice!.SwapChainSupportDetails.PresentModes);
            Extents = chooseExtents(_Context.PhysicalDevice!.SwapChainSupportDetails.SurfaceCapabilities);
            uint min_image_count = get_max_image_count_impl_impl(_Context.PhysicalDevice!.SwapChainSupportDetails.SurfaceCapabilities);

            SwapchainCreateInfoKHR swapchain_create_info = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _Context.Instance!.Surface,
                MinImageCount = min_image_count,
                ImageFormat = SurfaceFormat.Format,
                ImageColorSpace = SurfaceFormat.ColorSpace,
                ImageExtent = Extents,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = _Context.PhysicalDevice!.SwapChainSupportDetails.SurfaceCapabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                PresentMode = PresentMode,
                Clipped = true,
                OldSwapchain = default,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndexCount = 0,
                PQueueFamilyIndices = (uint*)null!
            };

            QueueFamilyIndices indices = _Context.PhysicalDevice!.GetQueueFamilies();

            if (indices.GraphicsFamily != indices.PresentationFamily)
            {
                swapchain_create_info.ImageSharingMode = SharingMode.Concurrent;
                swapchain_create_info.QueueFamilyIndexCount = 2;
                swapchain_create_info.PQueueFamilyIndices = (uint*)&indices;
            }

            Result result = _Context.LogicalDevice!.SwapchainExtension.CreateSwapchain(_Context.LogicalDevice!, &swapchain_create_info,
                (AllocationCallbacks*)null!, out SwapchainKHR swapchain_khr);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create logical device.");
            }

            _SwapchainKHR = swapchain_khr;
            _Images = new Image[min_image_count];

            fixed (Image* images_fixed = _Images)
            {
                _Context.LogicalDevice!.SwapchainExtension.GetSwapchainImages(_Context.LogicalDevice!, _SwapchainKHR, &min_image_count, images_fixed);
            }
        }


        #region Conversions

        public static implicit operator SwapchainKHR(VulkanSwapChain swapChain) => swapChain._SwapchainKHR;

        #endregion
    }
}
