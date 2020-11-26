using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.DirectX;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VKAPI : Singleton<VKAPI>
    {
#if DEBUG
        private const bool _ENABLE_VULKAN_VALIDATION = true;
#else
        private const bool _ENABLE_VULKAN_VALIDATION = false;
#endif

        private const int _MAX_FRAMES_IN_FLIGHT = 2;

        private static readonly string _VulkanSurfaceCreationFormat = $"({nameof(VKAPI)}) Creating surface: {{0}}";
        private static readonly string _VulkanDebugMessengerCreationFormat = $"({nameof(VKAPI)}) Creating debug messenger: {{0}}";
        private static readonly string _VulkanPhysicalDeviceSelectionFormat = $"({nameof(VKAPI)}) Selecting physical device: {{0}}";
        private static readonly string _VulkanLogicalDeviceCreationFormat = $"({nameof(VKAPI)}) Creating logical device: {{0}}";
        private static readonly string _VulkanSwapChainCreationFormat = $"({nameof(VKAPI)}) Creating swap chain: {{0}}";
        private static readonly string _VulkanImageViewCreationFormat = $"({nameof(VKAPI)}) Creating image views: {{0}}";
        private static readonly string _VulkanRenderPassCreationFormat = $"({nameof(VKAPI)}) Creating render pass: {{0}}";
        private static readonly string _VulkanGraphicsPipelineCreationFormat = $"({nameof(VKAPI)}) Creating graphics pipeline: {{0}}";
        private static readonly string _VulkanFramebuffersCreationFormat = $"({nameof(VKAPI)}) Creating frame buffers: {{0}}";
        private static readonly string _VulkanCommandPoolCreationFormat = $"({nameof(VKAPI)}) Creating command pool: {{0}}";
        private static readonly string _VulkanCommandBuffersCreationFormat = $"({nameof(VKAPI)}) Creating command buffers: {{0}}";
        private static readonly string _VulkanSemaphoreCreationFormat = $"({nameof(VKAPI)}) Creating semaphores: {{0}}";

        public static AllocationCallbacks AllocationCallback = new AllocationCallbacks();

        public static string[] ValidationLayers { get; } = {
            "VK_LAYER_KHRONOS_validation"
        };

        public static string[] DebugInstanceExtensions { get; } = {
            ExtDebugUtils.ExtensionName
        };

        public static readonly string[] LogicalDeviceExtensions =
        {
            KhrSwapchain.ExtensionName
        };

        private KhrSurface? _KHRSurface;
        private SurfaceKHR _Surface;

        private DebugUtilsMessengerEXT _DebugMessenger;
        private ExtDebugUtils? _ExtDebugUtils;

        private PhysicalDevice _PhysicalDevice;
        private QueueFamilyIndices _QueueFamilyIndices;
        private Device _LogicalDevice;

        private Queue _GraphicsQueue;
        private Queue _PresentationQueue;

        private SwapChainSupportDetails _SwapChainSupportDetails;
        private KhrSwapchain? _KHRSwapChain;
        private SwapchainKHR _SwapChain;
        private Format _SwapChainImageFormat;
        private Extent2D _SwapChainExtents;
        private Image[]? _SwapChainImages;

        private ImageView[]? _SwapChainImageViews;

        private RenderPass _RenderPass;
        private PipelineLayout _PipelineLayout;
        private Pipeline _GraphicsPipeline;
        private Framebuffer[] _SwapChainFramebuffers;

        private CommandPool _CommandPool;
        private CommandBuffer[] _CommandBuffers;

        private Semaphore[] _ImageAvailableSemaphores;
        private Semaphore[] _RenderFinishedSemaphores;

        private int _CurrentFrame;
        private Instance _VKInstance;

        public Vk VK { get; }

        public Instance VKInstance => _VKInstance;

        public VKAPI() => VK = Vk.GetApi();

        public static unsafe string[] GetRequiredExtensions(IVkSurface vkSurface, string[] requestedExtensions)
        {
            string[] requiredExtensions = SilkMarshal.MarshalPtrToStringArray((nint)vkSurface.GetRequiredExtensions(out uint extensionCount),
                (int)extensionCount);

            string[] extensions = new string[requiredExtensions.Length + requestedExtensions.Length];
            requestedExtensions.CopyTo(extensions, requiredExtensions.Length);
            requiredExtensions.CopyTo(extensions, 0);

            return extensions;
        }

        public static unsafe void VerifyValidationLayerSupport(Vk vk, IEnumerable<string> validationLayers)
        {
            uint layerCount = 0u;
            vk.EnumerateInstanceLayerProperties(&layerCount, (LayerProperties*)null!);
            Span<LayerProperties> layerProperties = stackalloc LayerProperties[(int)layerCount];
            vk.EnumerateInstanceLayerProperties(ref layerCount, ref layerProperties[0]);

            HashSet<string?> layers = new HashSet<string?>();

            foreach (LayerProperties layerPropertiesElement in layerProperties)
            {
                layers.Add(Marshal.PtrToStringAnsi((nint)layerPropertiesElement.LayerName));
            }

            foreach (string validationLayer in validationLayers)
            {
                if (!layers.Contains(validationLayer))
                {
                    throw new VulkanException(Result.ErrorLayerNotPresent, $"Validation layer '{validationLayer}' not present.");
                }
            }
        }

        public void DefaultInitialize()
        {
            Log.Information($"({nameof(VKAPI)}) Initializing Vulkan: -begin-");

            CreateSwapChain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateFramebuffers();
            CreateCommandPool();
            CreateCommandBuffers();
            CreateSemaphores();

            Log.Information($"({nameof(VKAPI)}) Initializing Vulkan: -success-");
        }

        public unsafe void DrawFrame()
        {
            Debug.Assert(_KHRSwapChain != null, "Field should already be initialized.");

            uint imageIndex = 0u;

            _KHRSwapChain.AcquireNextImage(_LogicalDevice, _SwapChain, ulong.MaxValue, _ImageAvailableSemaphores[_CurrentFrame], default,
                &imageIndex);

            Semaphore* waitSemaphores = stackalloc[]
            {
                _ImageAvailableSemaphores[_CurrentFrame]
            };

            Semaphore* signalSemaphores = stackalloc[]
            {
                _RenderFinishedSemaphores[_CurrentFrame]
            };

            PipelineStageFlags* waitStages = stackalloc[]
            {
                PipelineStageFlags.PipelineStageColorAttachmentOutputBit
            };

            SubmitInfo submitInfo = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSemaphores,
                PWaitDstStageMask = waitStages,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSemaphores
            };

            fixed (CommandBuffer* commandBufferFixed = &_CommandBuffers[imageIndex])
            {
                submitInfo.CommandBufferCount = 1;
                submitInfo.PCommandBuffers = commandBufferFixed;
            }

            if (VK.QueueSubmit(_GraphicsQueue, 1, &submitInfo, default) != Result.Success)
            {
                throw new Exception("Failed to submit draw command to command buffer.");
            }

            PresentInfoKHR presentInfo = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores
            };

            fixed (SwapchainKHR* swapChainFixed = &_SwapChain)
            {
                presentInfo.SwapchainCount = 1;
                presentInfo.PSwapchains = swapChainFixed;
                presentInfo.PImageIndices = &imageIndex;
            }

            presentInfo.PResults = null;

            _KHRSwapChain.QueuePresent(_PresentationQueue, &presentInfo);

            VK.QueueWaitIdle(_PresentationQueue);
            VK.DeviceWaitIdle(_LogicalDevice);

            _CurrentFrame = (_CurrentFrame + 1) % _MAX_FRAMES_IN_FLIGHT;
        }


        #region Create Swapchain

        private unsafe void CreateSwapChain()
        {
            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining optimal surface format."));
            SurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(_SwapChainSupportDetails.Formats);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining optimal surface presentation mode."));
            PresentModeKHR presentationMode = ChooseSwapPresentationMode(_SwapChainSupportDetails.PresentModes);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining extents."));
            Extent2D extents = ChooseSwapExtents(_SwapChainSupportDetails.SurfaceCapabilities);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining minimum image buffer length."));
            uint minImageCount = _SwapChainSupportDetails.SurfaceCapabilities.MinImageCount + 1;

            if ((_SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount > 0)
                && (minImageCount > _SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount))
            {
                minImageCount = _SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount;
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "initializing swap chain creation info."));

            SwapchainCreateInfoKHR swapChainCreateInfo = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _Surface,
                MinImageCount = minImageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extents,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = _SwapChainSupportDetails.SurfaceCapabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                PresentMode = presentationMode,
                Clipped = Vk.True,
                OldSwapchain = default
            };

            if (_QueueFamilyIndices.GraphicsFamily != _QueueFamilyIndices.PresentationFamily)
            {
                swapChainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                swapChainCreateInfo.QueueFamilyIndexCount = 2;

                Debug.Assert(_QueueFamilyIndices.GraphicsFamily.HasValue);
                Debug.Assert(_QueueFamilyIndices.PresentationFamily.HasValue);

                uint* indices = stackalloc uint[]
                {
                    _QueueFamilyIndices.GraphicsFamily.Value,
                    _QueueFamilyIndices.PresentationFamily.Value
                };

                swapChainCreateInfo.PQueueFamilyIndices = indices;
            }
            else
            {
                swapChainCreateInfo.ImageSharingMode = SharingMode.Exclusive;
                swapChainCreateInfo.QueueFamilyIndexCount = 0;
                swapChainCreateInfo.PQueueFamilyIndices = (uint*)null!;
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "creating swap chain."));

            fixed (SwapchainKHR* swapChainFixed = &_SwapChain)
            {
                if (_KHRSwapChain.CreateSwapchain(_LogicalDevice, &swapChainCreateInfo, (AllocationCallbacks*)null!, swapChainFixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create swap chain.");
                }
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "getting swap chain images."));

            _KHRSwapChain.GetSwapchainImages(_LogicalDevice, _SwapChain, &minImageCount, (Image*)null!);
            _SwapChainImages = new Image[minImageCount];

            fixed (Image* swapChainImagesFixed = _SwapChainImages)
            {
                _KHRSwapChain.GetSwapchainImages(_LogicalDevice, _SwapChain, &minImageCount, swapChainImagesFixed);
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "assigning global state variables."));

            _SwapChainImageFormat = surfaceFormat.Format;
            _SwapChainExtents = extents;

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "-success-"));
        }

        private static SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
        {
            foreach (SurfaceFormatKHR surfaceFormat in availableFormats)
            {
                if ((surfaceFormat.Format == Format.B8G8R8Srgb) && (surfaceFormat.ColorSpace == ColorSpaceKHR.ColorspaceSrgbNonlinearKhr))
                {
                    return surfaceFormat;
                }
            }

            return availableFormats[0];
        }

        private static PresentModeKHR ChooseSwapPresentationMode(IEnumerable<PresentModeKHR> availablePresentationModes)
        {
            foreach (PresentModeKHR presentationMode in availablePresentationModes)
            {
                if (presentationMode == PresentModeKHR.PresentModeMailboxKhr)
                {
                    return presentationMode;
                }
            }

            return PresentModeKHR.PresentModeFifoKhr;
        }

        private static Extent2D ChooseSwapExtents(SurfaceCapabilitiesKHR surfaceCapabilities)
        {
            if (surfaceCapabilities.CurrentExtent.Width != int.MaxValue)
            {
                return surfaceCapabilities.CurrentExtent;
            }
            else
            {
                Extent2D adjustedExtent = new Extent2D((uint)AutomataWindow.Instance.Size.X, (uint)AutomataWindow.Instance.Size.Y);

                adjustedExtent.Width = Math.Max(surfaceCapabilities.MinImageExtent.Width,
                    Math.Min(surfaceCapabilities.MinImageExtent.Width, adjustedExtent.Width));

                adjustedExtent.Height = Math.Max(surfaceCapabilities.MinImageExtent.Height,
                    Math.Min(surfaceCapabilities.MinImageExtent.Height, adjustedExtent.Height));

                return adjustedExtent;
            }
        }

        #endregion


        #region Create Image Views

        private unsafe void CreateImageViews()
        {
            Log.Information(string.Format(_VulkanImageViewCreationFormat, "-begin-"));

            _SwapChainImageViews = new ImageView[_SwapChainImages.Length];

            for (int index = 0; index < _SwapChainImageViews.Length; index++)
            {
                Log.Information(string.Format(_VulkanImageViewCreationFormat, $"initializing image view creation info ({index})."));

                ImageViewCreateInfo imageViewCreateInfo = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = _SwapChainImages[index],
                    ViewType = ImageViewType.ImageViewType2D,
                    Format = _SwapChainImageFormat,
                    Components = new ComponentMapping(),
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0u, 1u, 0u, 1u)
                };

                Log.Information(string.Format(_VulkanImageViewCreationFormat, $"creating and assigning image view ({index})."));

                fixed (ImageView* swapChainImageViews = &_SwapChainImageViews[index])
                {
                    if (VK.CreateImageView(_LogicalDevice, &imageViewCreateInfo, (AllocationCallbacks*)null!, swapChainImageViews)
                        != Result.Success)
                    {
                        throw new Exception($"Failed to create image views for index {index}.");
                    }
                }
            }

            Log.Information(string.Format(_VulkanImageViewCreationFormat, "-success-"));
        }

        #endregion


        #region Render Pass

        private unsafe void CreateRenderPass()
        {
            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating attachment descriptions."));

            AttachmentDescription colorAttachment = new AttachmentDescription
            {
                Format = _SwapChainImageFormat,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr
            };

            AttachmentReference subpassAttachmentReference = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal
            };

            SubpassDescription subpassDescription = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &subpassAttachmentReference
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating subpass dependency information."));

            SubpassDependency subpassDependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating render pass information."));

            RenderPassCreateInfo renderPassCreateInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpassDescription,
                DependencyCount = 1,
                PDependencies = &subpassDependency
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "assigning render pass."));

            fixed (RenderPass* renderPassFixed = &_RenderPass)
            {
                if (VK.CreateRenderPass(_LogicalDevice, &renderPassCreateInfo, (AllocationCallbacks*)null!, renderPassFixed) != Result.Success)
                {
                    throw new Exception("Failed to create render pass.");
                }
            }

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "-success-"));
        }

        #endregion


        #region Create Graphics Pipeline

        private unsafe void CreateGraphicsPipeline()
        {
            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "loading default shaders."));

            ShaderModule vertexShader = CreateShaderModule((byte[])GLSLXPLR.Instance.DefaultVertexShader);
            ShaderModule fragmentShader = CreateShaderModule((byte[])GLSLXPLR.Instance.DefaultFragmentShader);

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating shader stage information."));

            PipelineShaderStageCreateInfo vertexShaderStageCreateInfo = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageVertexBit,
                PName = (byte*)SilkMarshal.MarshalStringToPtr("main"),
                Module = vertexShader,
                PSpecializationInfo = null
            };

            PipelineShaderStageCreateInfo fragmentShaderStageCreateInfo = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageFragmentBit,
                PName = (byte*)SilkMarshal.MarshalStringToPtr("main"),
                Module = fragmentShader,
                PSpecializationInfo = null
            };

            PipelineShaderStageCreateInfo* shaderStages = stackalloc[]
            {
                vertexShaderStageCreateInfo,
                fragmentShaderStageCreateInfo
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating vertex stage information."));

            PipelineVertexInputStateCreateInfo vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexAttributeDescriptionCount = 0,
                PVertexAttributeDescriptions = null,
                VertexBindingDescriptionCount = 0,
                PVertexBindingDescriptions = null
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating assembly stage information."));

            PipelineInputAssemblyStateCreateInfo assemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = Vk.False
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating viewport information."));

            Viewport viewport = new Viewport
            {
                X = 0f,
                Y = 0f,
                Width = (float)_SwapChainExtents.Width,
                Height = (float)_SwapChainExtents.Height,
                MinDepth = 0f,
                MaxDepth = 1f
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating viewport scissor information."));

            Rect2D scissor = new Rect2D
            {
                Offset = new Offset2D(0),
                Extent = _SwapChainExtents
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "aggregating viewport state information."));

            PipelineViewportStateCreateInfo viewportStateCreateInfo = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating rasterization information."));

            PipelineRasterizationStateCreateInfo rasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = Vk.False,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1f,
                CullMode = CullModeFlags.CullModeBackBit,
                FrontFace = FrontFace.Clockwise,
                DepthBiasEnable = Vk.False,
                DepthBiasConstantFactor = 0f,
                DepthBiasClamp = 0f,
                DepthBiasSlopeFactor = 0f
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating multisampling information."));

            PipelineMultisampleStateCreateInfo multisampleStateCreateInfo = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = Vk.False,
                RasterizationSamples = SampleCountFlags.SampleCount1Bit,
                MinSampleShading = 1f,
                PSampleMask = null,
                AlphaToCoverageEnable = Vk.False,
                AlphaToOneEnable = Vk.False
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating color blender information."));

            PipelineColorBlendAttachmentState colorBlendAttachmentState = new PipelineColorBlendAttachmentState
            {
                ColorWriteMask = ColorComponentFlags.ColorComponentRBit
                                 | ColorComponentFlags.ColorComponentGBit
                                 | ColorComponentFlags.ColorComponentBBit
                                 | ColorComponentFlags.ColorComponentABit,
                BlendEnable = Vk.False,
                SrcColorBlendFactor = BlendFactor.One,
                DstColorBlendFactor = BlendFactor.Zero,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.Zero,
                AlphaBlendOp = BlendOp.Add
            };

            PipelineColorBlendStateCreateInfo colorBlendStateCreateInfo = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = Vk.False,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachmentState
            };

            colorBlendStateCreateInfo.BlendConstants[0] =
                colorBlendStateCreateInfo.BlendConstants[1] =
                    colorBlendStateCreateInfo.BlendConstants[2] =
                        colorBlendStateCreateInfo.BlendConstants[3] = 0.0f;

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "configuring dynamic state."));

            DynamicState* dynamicStates = stackalloc[]
            {
                DynamicState.Viewport
            };

            PipelineDynamicStateCreateInfo dynamicStateCreateInfo = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 1,
                PDynamicStates = dynamicStates
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating pipeline layout information."));

            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 0,
                PSetLayouts = null,
                PushConstantRangeCount = 0,
                PPushConstantRanges = null
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "assigning pipeline layout."));

            fixed (PipelineLayout* pipelineLayoutFixed = &_PipelineLayout)
            {
                if (VK.CreatePipelineLayout(_LogicalDevice, &pipelineLayoutCreateInfo, (AllocationCallbacks*)null!, pipelineLayoutFixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create pipeline layout.");
                }
            }

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating final graphics pipeline."));

            GraphicsPipelineCreateInfo graphicsPipelineCreateInfo = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertexInputStateCreateInfo,
                PInputAssemblyState = &assemblyStateCreateInfo,
                PViewportState = &viewportStateCreateInfo,
                PRasterizationState = &rasterizationStateCreateInfo,
                PMultisampleState = &multisampleStateCreateInfo,
                PDepthStencilState = null,
                PColorBlendState = &colorBlendStateCreateInfo,
                PDynamicState = null,
                Layout = _PipelineLayout,
                RenderPass = _RenderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "assigning graphics pipeline."));

            fixed (Pipeline* graphicsPipelineFixed = &_GraphicsPipeline)
            {
                if (VK.CreateGraphicsPipelines(_LogicalDevice, default, 1, &graphicsPipelineCreateInfo, (AllocationCallbacks*)null!,
                        graphicsPipelineFixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create graphics pipeline.");
                }
            }

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "destroying shader modules."));

            VK.DestroyShaderModule(_LogicalDevice, vertexShader, (AllocationCallbacks*)null!);
            VK.DestroyShaderModule(_LogicalDevice, fragmentShader, (AllocationCallbacks*)null!);

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "-success-"));
        }

        private unsafe ShaderModule CreateShaderModule(byte[] byteCode)
        {
            ShaderModuleCreateInfo shaderModuleCreateInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (UIntPtr)byteCode.Length
            };

            fixed (byte* byteCodeFixed = byteCode)
            {
                shaderModuleCreateInfo.PCode = (uint*)byteCodeFixed;
            }

            ShaderModule shaderModule;

            if (VK.CreateShaderModule(_LogicalDevice, &shaderModuleCreateInfo, (AllocationCallbacks*)null!, &shaderModule) != Result.Success)
            {
                throw new Exception("Failed to create shader module.");
            }

            return shaderModule;
        }

        #endregion


        #region Create Framebuffers

        private unsafe void CreateFramebuffers()
        {
            Debug.Assert(_SwapChainImageViews != null, $"{nameof(_SwapChainImageViews)} should have already been initialized.");

            Log.Information(string.Format(_VulkanFramebuffersCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanFramebuffersCreationFormat, "resizing framebuffer array."));

            _SwapChainFramebuffers = new Framebuffer[_SwapChainImageViews.Length];

            Log.Information(string.Format(_VulkanFramebuffersCreationFormat, $"creating {_SwapChainFramebuffers.Length} framebuffers."));

            for (int index = 0; index < _SwapChainFramebuffers.Length; index++)
            {
                ImageView* attachments = stackalloc[]
                {
                    _SwapChainImageViews[index]
                };

                FramebufferCreateInfo framebufferCreateInfo = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = _RenderPass,
                    AttachmentCount = 1,
                    PAttachments = attachments,
                    Width = _SwapChainExtents.Width,
                    Height = _SwapChainExtents.Height,
                    Layers = 1
                };

                Log.Information(string.Format(_VulkanFramebuffersCreationFormat, $"assigning framebuffer {index}."));

                fixed (Framebuffer* swapChainFramebuffersFixed = _SwapChainFramebuffers)
                {
                    if (VK.CreateFramebuffer(_LogicalDevice, &framebufferCreateInfo, (AllocationCallbacks*)null!, &swapChainFramebuffersFixed[index])
                        != Result.Success)
                    {
                        throw new Exception($"Failed to create framebuffer (index {index}).");
                    }
                }
            }

            Log.Information(string.Format(_VulkanFramebuffersCreationFormat, "-success-"));
        }

        #endregion


        #region Create Command Pool

        private unsafe void CreateCommandPool()
        {
            Debug.Assert(_QueueFamilyIndices.GraphicsFamily != null, "Value should already be initialized.");

            Log.Information(string.Format(_VulkanCommandPoolCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanCommandPoolCreationFormat, "creating command pool."));

            CommandPoolCreateInfo commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = _QueueFamilyIndices.GraphicsFamily.Value,
                Flags = 0
            };

            Log.Information(string.Format(_VulkanCommandPoolCreationFormat, "assigning command pool."));

            fixed (CommandPool* commandPoolFixed = &_CommandPool)
            {
                if (VK.CreateCommandPool(_LogicalDevice, &commandPoolCreateInfo, (AllocationCallbacks*)null!, commandPoolFixed) != Result.Success)
                {
                    throw new Exception("Failed to create command pool.");
                }
            }

            Log.Information(string.Format(_VulkanCommandPoolCreationFormat, "-success-"));
        }

        #endregion


        #region CreateCommandBuffers

        private unsafe void CreateCommandBuffers()
        {
            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "resizing command buffers array."));

            _CommandBuffers = new CommandBuffer[_SwapChainFramebuffers.Length];

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "creating command buffers."));

            CommandBufferAllocateInfo commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _CommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)_CommandBuffers.Length
            };

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "assigning command buffers."));

            fixed (CommandBuffer* commandBuffersFixed = _CommandBuffers)
            {
                if (VK.AllocateCommandBuffers(_LogicalDevice, &commandBufferAllocateInfo, commandBuffersFixed) != Result.Success)
                {
                    throw new Exception("Failed to create command buffers.");
                }
            }

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "configuring command buffers."));

            for (int index = 0; index < _CommandBuffers.Length; index++)
            {
                CommandBufferBeginInfo commandBufferBeginInfo = new CommandBufferBeginInfo
                {
                    SType = StructureType.CommandBufferBeginInfo,
                    Flags = 0,
                    PInheritanceInfo = null
                };

                if (VK.BeginCommandBuffer(_CommandBuffers[index], &commandBufferBeginInfo) != Result.Success)
                {
                    throw new Exception("Failed to begin recording command buffer.");
                }

                ClearValue clearValue = new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f));

                RenderPassBeginInfo renderPassBeginInfo = new RenderPassBeginInfo
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = _RenderPass,
                    Framebuffer = _SwapChainFramebuffers[index],
                    RenderArea = new Rect2D(new Offset2D(0), _SwapChainExtents),
                    ClearValueCount = 1,
                    PClearValues = &clearValue
                };

                VK.CmdBeginRenderPass(_CommandBuffers[index], &renderPassBeginInfo, SubpassContents.Inline);
                VK.CmdBindPipeline(_CommandBuffers[index], PipelineBindPoint.Graphics, _GraphicsPipeline);
                VK.CmdDraw(_CommandBuffers[index], 3, 1, 0, 0);
                VK.CmdEndRenderPass(_CommandBuffers[index]);

                if (VK.EndCommandBuffer(_CommandBuffers[index]) != Result.Success)
                {
                    throw new Exception("Failed to record command buffer.");
                }
            }

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "-success-"));
        }

        #endregion


        #region Create Semaphores

        private unsafe void CreateSemaphores()
        {
            Log.Information(string.Format(_VulkanSemaphoreCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanSemaphoreCreationFormat, "creating semaphores."));

            SemaphoreCreateInfo semaphoreCreateInfo = new SemaphoreCreateInfo
            {
                SType = StructureType.SemaphoreCreateInfo
            };

            _ImageAvailableSemaphores = new Semaphore[_MAX_FRAMES_IN_FLIGHT];
            _RenderFinishedSemaphores = new Semaphore[_MAX_FRAMES_IN_FLIGHT];

            fixed (Semaphore* imageAvailableSemaphoresFixed = _ImageAvailableSemaphores)
            {
                fixed (Semaphore* renderFinishedSemaphoresFixed = _RenderFinishedSemaphores)
                {
                    for (int index = 0; index < _MAX_FRAMES_IN_FLIGHT; index++)
                    {
                        if (VK.CreateSemaphore(_LogicalDevice, &semaphoreCreateInfo, (AllocationCallbacks*)null!,
                                &imageAvailableSemaphoresFixed[index])
                            != Result.Success)
                        {
                            throw new Exception("Failed to create image availability semaphore.");
                        }

                        if (VK.CreateSemaphore(_LogicalDevice, &semaphoreCreateInfo, (AllocationCallbacks*)null!,
                                &renderFinishedSemaphoresFixed[index])
                            != Result.Success)
                        {
                            throw new Exception("Failed to create render finished semaphore.");
                        }
                    }
                }
            }

            Log.Information(string.Format(_VulkanSemaphoreCreationFormat, "-success-"));
        }

        #endregion


        public unsafe void DestroyVulkanInstance()
        {
            for (int index = 0; index < _MAX_FRAMES_IN_FLIGHT; index++)
            {
                VK.DestroySemaphore(_LogicalDevice, _ImageAvailableSemaphores[index], (AllocationCallbacks*)null!);
                VK.DestroySemaphore(_LogicalDevice, _RenderFinishedSemaphores[index], (AllocationCallbacks*)null!);
            }

            VK.DestroyCommandPool(_LogicalDevice, _CommandPool, (AllocationCallbacks*)null!);

            foreach (Framebuffer framebuffer in _SwapChainFramebuffers)
            {
                VK.DestroyFramebuffer(_LogicalDevice, framebuffer, (AllocationCallbacks*)null!);
            }

            VK.DestroyPipeline(_LogicalDevice, _GraphicsPipeline, (AllocationCallbacks*)null!);
            VK.DestroyRenderPass(_LogicalDevice, _RenderPass, (AllocationCallbacks*)null!);
            VK.DestroyPipelineLayout(_LogicalDevice, _PipelineLayout, (AllocationCallbacks*)null!);

            foreach (ImageView imageView in _SwapChainImageViews)
            {
                VK.DestroyImageView(_LogicalDevice, imageView, (AllocationCallbacks*)null!);
            }

            _KHRSwapChain.DestroySwapchain(_LogicalDevice, _SwapChain, (AllocationCallbacks*)null!);
            VK.DestroyDevice(_LogicalDevice, (AllocationCallbacks*)null!);

            if (_ENABLE_VULKAN_VALIDATION)
            {
                _ExtDebugUtils.DestroyDebugUtilsMessenger(VKInstance, _DebugMessenger, (AllocationCallbacks*)null!);
            }

            _KHRSurface.DestroySurface(VKInstance, _Surface, (AllocationCallbacks*)null!);
            VK.DestroyInstance(VKInstance, (AllocationCallbacks*)null!);
        }

        ~VKAPI() { DestroyVulkanInstance(); }
    }
}
