using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Ultz.SuperInvoke;

namespace Automata.Engine.Rendering.Vulkan
{
    [Extension(EXTENSION_NAME)]
    public abstract class SwapchainExtension : NativeExtension<Vk>
    {
        public const string EXTENSION_NAME = "VK_KHR_swapchain";

        public SwapchainExtension(ref NativeApiContext ctx) : base(ref ctx) { }

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkAcquireNextImageKHR")]
        public abstract unsafe Result AcquireNextImage(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0)] ulong timeout,
            [Count(Count = 0)] Semaphore semaphore,
            [Count(Count = 0)] Fence fence,
            [Count(Count = 0)] uint* pImageIndex);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkAcquireNextImageKHR")]
        public abstract Result AcquireNextImage(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0)] ulong timeout,
            [Count(Count = 0)] Semaphore semaphore,
            [Count(Count = 0)] Fence fence,
            [Count(Count = 0)] ref uint pImageIndex);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkAcquireNextImage2KHR")]
        public abstract unsafe Result AcquireNextImage2(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.In)]
            AcquireNextImageInfoKHR* pAcquireInfo,
            [Count(Count = 0)] uint* pImageIndex);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkAcquireNextImage2KHR")]
        public abstract Result AcquireNextImage2(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.In)]
            ref AcquireNextImageInfoKHR pAcquireInfo,
            [Count(Count = 0)] ref uint pImageIndex);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkCreateSwapchainKHR")]
        public abstract unsafe Result CreateSwapchain(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.In)]
            SwapchainCreateInfoKHR* pCreateInfo,
            [Count(Count = 0), Flow(FlowDirection.In)]
            AllocationCallbacks* pAllocator,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            SwapchainKHR* pSwapchain);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkCreateSwapchainKHR")]
        public abstract Result CreateSwapchain(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.In)]
            ref SwapchainCreateInfoKHR pCreateInfo,
            [Count(Count = 0), Flow(FlowDirection.In)]
            ref AllocationCallbacks pAllocator,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            out SwapchainKHR pSwapchain);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkDestroySwapchainKHR")]
        public abstract unsafe void DestroySwapchain(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0), Flow(FlowDirection.In)]
            AllocationCallbacks* pAllocator);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkDestroySwapchainKHR")]
        public abstract void DestroySwapchain(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0), Flow(FlowDirection.In)]
            ref AllocationCallbacks pAllocator);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetDeviceGroupPresentCapabilitiesKHR")]
        public abstract unsafe Result GetDeviceGroupPresentCapabilities(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            DeviceGroupPresentCapabilitiesKHR* pDeviceGroupPresentCapabilities);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetDeviceGroupPresentCapabilitiesKHR")]
        public abstract Result GetDeviceGroupPresentCapabilities(
            [Count(Count = 0)] Device device,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            out DeviceGroupPresentCapabilitiesKHR pDeviceGroupPresentCapabilities);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetDeviceGroupSurfacePresentModesKHR")]
        public abstract unsafe Result GetDeviceGroupSurfacePresentModes(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SurfaceKHR surface,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            DeviceGroupPresentModeFlagsKHR* pModes);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetDeviceGroupSurfacePresentModesKHR")]
        public abstract Result GetDeviceGroupSurfacePresentModes(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SurfaceKHR surface,
            [Count(Count = 0), Flow(FlowDirection.Out)]
            out DeviceGroupPresentModeFlagsKHR pModes);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetPhysicalDevicePresentRectanglesKHR")]
        public abstract unsafe Result GetPhysicalDevicePresentRectangles(
            [Count(Count = 0)] PhysicalDevice physicalDevice,
            [Count(Count = 0)] SurfaceKHR surface,
            [Count(Count = 0)] uint* pRectCount,
            [Count(Computed = "pRectCount"), Flow(FlowDirection.Out)]
            Rect2D* pRects);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetPhysicalDevicePresentRectanglesKHR")]
        public abstract Result GetPhysicalDevicePresentRectangles(
            [Count(Count = 0)] PhysicalDevice physicalDevice,
            [Count(Count = 0)] SurfaceKHR surface,
            [Count(Count = 0)] ref uint pRectCount,
            [Count(Computed = "pRectCount"), Flow(FlowDirection.Out)]
            out Rect2D pRects);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetSwapchainImagesKHR")]
        public abstract unsafe Result GetSwapchainImages(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0)] uint* pSwapchainImageCount,
            [Count(Computed = "pSwapchainImageCount"), Flow(FlowDirection.Out)]
            Image* pSwapchainImages);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkGetSwapchainImagesKHR")]
        public abstract Result GetSwapchainImages(
            [Count(Count = 0)] Device device,
            [Count(Count = 0)] SwapchainKHR swapchain,
            [Count(Count = 0)] ref uint pSwapchainImageCount,
            [Count(Computed = "pSwapchainImageCount"), Flow(FlowDirection.Out)]
            out Image pSwapchainImages);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkQueuePresentKHR")]
        public abstract unsafe Result QueuePresent([Count(Count = 0)] Queue queue, [Count(Count = 0), Flow(FlowDirection.In)]
            PresentInfoKHR* pPresentInfo);

        /// <summary>To be added.</summary>
        [NativeApi(EntryPoint = "vkQueuePresentKHR")]
        public abstract Result QueuePresent([Count(Count = 0)] Queue queue, [Count(Count = 0), Flow(FlowDirection.In)]
            ref PresentInfoKHR pPresentInfo);
    }
}
