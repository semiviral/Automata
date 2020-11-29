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
            static uint GetMaxImageCountImpl(SurfaceCapabilitiesKHR surfaceCapabilities)
            {
                uint minImageCount = surfaceCapabilities.MinImageCount + 1;

                if ((surfaceCapabilities.MinImageCount > 0) && (minImageCount > surfaceCapabilities.MaxImageCount))
                {
                    minImageCount = surfaceCapabilities.MaxImageCount;
                }

                return minImageCount;
            }

            _Context = context;
            SurfaceFormat = chooseFormat(_Context.PhysicalDevice!.SwapChainSupportDetails.Formats);
            PresentMode = choosePresentMode(_Context.PhysicalDevice!.SwapChainSupportDetails.PresentModes);
            Extents = chooseExtents(_Context.PhysicalDevice!.SwapChainSupportDetails.SurfaceCapabilities);
            uint minImageCount = GetMaxImageCountImpl(_Context.PhysicalDevice!.SwapChainSupportDetails.SurfaceCapabilities);

            SwapchainCreateInfoKHR swapchainCreateInfo = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _Context.Instance!.Surface,
                MinImageCount = minImageCount,
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
                swapchainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                swapchainCreateInfo.QueueFamilyIndexCount = 2;
                swapchainCreateInfo.PQueueFamilyIndices = (uint*)&indices;
            }

            Result result = _Context.LogicalDevice!.SwapchainExtension.CreateSwapchain(_Context.LogicalDevice!, &swapchainCreateInfo,
                (AllocationCallbacks*)null!, out SwapchainKHR swapchainKHR);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create logical device.");
            }

            _SwapchainKHR = swapchainKHR;
            _Images = new Image[minImageCount];

            fixed (Image* imagesFixed = _Images)
            {
                _Context.LogicalDevice!.SwapchainExtension.GetSwapchainImages(_Context.LogicalDevice!, _SwapchainKHR, &minImageCount, imagesFixed);
            }
        }


        #region Conversions

        public static implicit operator SwapchainKHR(VulkanSwapChain swapChain) => swapChain._SwapchainKHR;

        #endregion
    }
}
