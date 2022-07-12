using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Automata.Engine.Rendering.DirectX;
using Automata.Engine.Rendering.Vulkan.NativeExtensions;
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

        private static readonly string _VulkanSwapChainCreationFormat = $"({nameof(VKAPI)}) Creating swap chain: {{0}}";
        private static readonly string _VulkanImageViewCreationFormat = $"({nameof(VKAPI)}) Creating image views: {{0}}";
        private static readonly string _VulkanRenderPassCreationFormat = $"({nameof(VKAPI)}) Creating render pass: {{0}}";
        private static readonly string _VulkanGraphicsPipelineCreationFormat = $"({nameof(VKAPI)}) Creating graphics pipeline: {{0}}";
        private static readonly string _VulkanFramebuffersCreationFormat = $"({nameof(VKAPI)}) Creating frame buffers: {{0}}";
        private static readonly string _VulkanCommandPoolCreationFormat = $"({nameof(VKAPI)}) Creating command pool: {{0}}";
        private static readonly string _VulkanCommandBuffersCreationFormat = $"({nameof(VKAPI)}) Creating command buffers: {{0}}";
        private static readonly string _VulkanSemaphoreCreationFormat = $"({nameof(VKAPI)}) Creating semaphores: {{0}}";

        public static AllocationCallbacks AllocationCallback = new AllocationCallbacks();

        public static string[] ValidationLayers { get; } =
        {
            "VK_LAYER_KHRONOS_validation"
        };

        public static string[] DebugInstanceExtensions { get; } =
        {
            ExtDebugUtils.ExtensionName
        };

        public static readonly string[] LogicalDeviceExtensions =
        {
            SwapchainExtension.ExtensionName
        };

        private KhrSurface? _KHRSurface;
        private SurfaceKHR _Surface;

        private DebugUtilsMessengerEXT _DebugMessenger;
        private ExtDebugUtils? _ExtDebugUtils;

        private PhysicalDevice _PhysicalDevice;
        private readonly QueueFamilyIndices _QueueFamilyIndices;
        private Device _LogicalDevice;

        private Queue _GraphicsQueue;
        private Queue _PresentationQueue;

        private readonly SwapChainSupportDetails _SwapChainSupportDetails;
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
            string[] required_extensions = SilkMarshal.MarshalPtrToStringArray((nint)vkSurface.GetRequiredExtensions(out uint extension_count),
                (int)extension_count);

            string[] extensions = new string[required_extensions.Length + requestedExtensions.Length];
            requestedExtensions.CopyTo(extensions, required_extensions.Length);
            required_extensions.CopyTo(extensions, 0);

            return extensions;
        }

        public static unsafe void VerifyValidationLayerSupport(Vk vk, IEnumerable<string> validationLayers)
        {
            uint layer_count = 0u;
            vk.EnumerateInstanceLayerProperties(&layer_count, (LayerProperties*)null!);
            Span<LayerProperties> layer_properties = stackalloc LayerProperties[(int)layer_count];
            vk.EnumerateInstanceLayerProperties(ref layer_count, ref layer_properties[0]);

            HashSet<string?> layers = new HashSet<string?>();

            foreach (LayerProperties layer_properties_element in layer_properties)
            {
                layers.Add(Marshal.PtrToStringAnsi((nint)layer_properties_element.LayerName));
            }

            foreach (string validation_layer in validationLayers)
            {
                if (!layers.Contains(validation_layer))
                {
                    throw new VulkanException(Result.ErrorLayerNotPresent, $"Validation layer '{validation_layer}' not present.");
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

            uint image_index = 0u;

            _KHRSwapChain.AcquireNextImage(_LogicalDevice, _SwapChain, ulong.MaxValue, _ImageAvailableSemaphores[_CurrentFrame], default,
                &image_index);

            Semaphore* wait_semaphores = stackalloc[]
            {
                _ImageAvailableSemaphores[_CurrentFrame]
            };

            Semaphore* signal_semaphores = stackalloc[]
            {
                _RenderFinishedSemaphores[_CurrentFrame]
            };

            PipelineStageFlags* wait_stages = stackalloc[]
            {
                PipelineStageFlags.PipelineStageColorAttachmentOutputBit
            };

            SubmitInfo submit_info = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = wait_semaphores,
                PWaitDstStageMask = wait_stages,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signal_semaphores
            };

            fixed (CommandBuffer* command_buffer_fixed = &_CommandBuffers[image_index])
            {
                submit_info.CommandBufferCount = 1;
                submit_info.PCommandBuffers = command_buffer_fixed;
            }

            if (VK.QueueSubmit(_GraphicsQueue, 1, &submit_info, default) != Result.Success)
            {
                throw new Exception("Failed to submit draw command to command buffer.");
            }

            PresentInfoKHR present_info = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = signal_semaphores
            };

            fixed (SwapchainKHR* swap_chain_fixed = &_SwapChain)
            {
                present_info.SwapchainCount = 1;
                present_info.PSwapchains = swap_chain_fixed;
                present_info.PImageIndices = &image_index;
            }

            present_info.PResults = null;

            _KHRSwapChain.QueuePresent(_PresentationQueue, &present_info);

            VK.QueueWaitIdle(_PresentationQueue);
            VK.DeviceWaitIdle(_LogicalDevice);

            _CurrentFrame = (_CurrentFrame + 1) % _MAX_FRAMES_IN_FLIGHT;
        }


        #region Create Swapchain

        private unsafe void CreateSwapChain()
        {
            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining optimal surface format."));
            SurfaceFormatKHR surface_format = ChooseSwapSurfaceFormat(_SwapChainSupportDetails.Formats);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining optimal surface presentation mode."));
            PresentModeKHR presentation_mode = ChooseSwapPresentationMode(_SwapChainSupportDetails.PresentModes);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining extents."));
            Extent2D extents = ChooseSwapExtents(_SwapChainSupportDetails.SurfaceCapabilities);

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "determining minimum image buffer length."));
            uint min_image_count = _SwapChainSupportDetails.SurfaceCapabilities.MinImageCount + 1;

            if ((_SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount > 0)
                && (min_image_count > _SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount))
            {
                min_image_count = _SwapChainSupportDetails.SurfaceCapabilities.MaxImageCount;
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "initializing swap chain creation info."));

            SwapchainCreateInfoKHR swap_chain_create_info = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _Surface,
                MinImageCount = min_image_count,
                ImageFormat = surface_format.Format,
                ImageColorSpace = surface_format.ColorSpace,
                ImageExtent = extents,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,
                PreTransform = _SwapChainSupportDetails.SurfaceCapabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
                PresentMode = presentation_mode,
                Clipped = Vk.True,
                OldSwapchain = default
            };

            if (_QueueFamilyIndices.GraphicsFamily != _QueueFamilyIndices.PresentationFamily)
            {
                swap_chain_create_info.ImageSharingMode = SharingMode.Concurrent;
                swap_chain_create_info.QueueFamilyIndexCount = 2;

                Debug.Assert(_QueueFamilyIndices.GraphicsFamily.HasValue);
                Debug.Assert(_QueueFamilyIndices.PresentationFamily.HasValue);

                uint* indices = stackalloc uint[]
                {
                    _QueueFamilyIndices.GraphicsFamily.Value,
                    _QueueFamilyIndices.PresentationFamily.Value
                };

                swap_chain_create_info.PQueueFamilyIndices = indices;
            }
            else
            {
                swap_chain_create_info.ImageSharingMode = SharingMode.Exclusive;
                swap_chain_create_info.QueueFamilyIndexCount = 0;
                swap_chain_create_info.PQueueFamilyIndices = (uint*)null!;
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "creating swap chain."));

            fixed (SwapchainKHR* swap_chain_fixed = &_SwapChain)
            {
                if (_KHRSwapChain.CreateSwapchain(_LogicalDevice, &swap_chain_create_info, (AllocationCallbacks*)null!, swap_chain_fixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create swap chain.");
                }
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "getting swap chain images."));

            _KHRSwapChain.GetSwapchainImages(_LogicalDevice, _SwapChain, &min_image_count, (Image*)null!);
            _SwapChainImages = new Image[min_image_count];

            fixed (Image* swap_chain_images_fixed = _SwapChainImages)
            {
                _KHRSwapChain.GetSwapchainImages(_LogicalDevice, _SwapChain, &min_image_count, swap_chain_images_fixed);
            }

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "assigning global state variables."));

            _SwapChainImageFormat = surface_format.Format;
            _SwapChainExtents = extents;

            Log.Information(string.Format(_VulkanSwapChainCreationFormat, "-success-"));
        }

        private static SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
        {
            foreach (SurfaceFormatKHR surface_format in availableFormats)
            {
                if ((surface_format.Format == Format.B8G8R8Srgb) && (surface_format.ColorSpace == ColorSpaceKHR.ColorspaceSrgbNonlinearKhr))
                {
                    return surface_format;
                }
            }

            return availableFormats[0];
        }

        private static PresentModeKHR ChooseSwapPresentationMode(IEnumerable<PresentModeKHR> availablePresentationModes)
        {
            foreach (PresentModeKHR presentation_mode in availablePresentationModes)
            {
                if (presentation_mode == PresentModeKHR.PresentModeMailboxKhr)
                {
                    return presentation_mode;
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
                Extent2D adjusted_extent = new Extent2D((uint)AutomataWindow.Instance.Size.X, (uint)AutomataWindow.Instance.Size.Y);

                adjusted_extent.Width = Math.Max(surfaceCapabilities.MinImageExtent.Width,
                    Math.Min(surfaceCapabilities.MinImageExtent.Width, adjusted_extent.Width));

                adjusted_extent.Height = Math.Max(surfaceCapabilities.MinImageExtent.Height,
                    Math.Min(surfaceCapabilities.MinImageExtent.Height, adjusted_extent.Height));

                return adjusted_extent;
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

                ImageViewCreateInfo image_view_create_info = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = _SwapChainImages[index],
                    ViewType = ImageViewType.ImageViewType2D,
                    Format = _SwapChainImageFormat,
                    Components = new ComponentMapping(),
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0u, 1u, 0u, 1u)
                };

                Log.Information(string.Format(_VulkanImageViewCreationFormat, $"creating and assigning image view ({index})."));

                fixed (ImageView* swap_chain_image_views = &_SwapChainImageViews[index])
                {
                    if (VK.CreateImageView(_LogicalDevice, &image_view_create_info, (AllocationCallbacks*)null!, swap_chain_image_views)
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

            AttachmentDescription color_attachment = new AttachmentDescription
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

            AttachmentReference subpass_attachment_reference = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal
            };

            SubpassDescription subpass_description = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &subpass_attachment_reference
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating subpass dependency information."));

            SubpassDependency subpass_dependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating render pass information."));

            RenderPassCreateInfo render_pass_create_info = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &color_attachment,
                SubpassCount = 1,
                PSubpasses = &subpass_description,
                DependencyCount = 1,
                PDependencies = &subpass_dependency
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "assigning render pass."));

            fixed (RenderPass* render_pass_fixed = &_RenderPass)
            {
                if (VK.CreateRenderPass(_LogicalDevice, &render_pass_create_info, (AllocationCallbacks*)null!, render_pass_fixed) != Result.Success)
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

            ShaderModule vertex_shader = CreateShaderModule((byte[])GLSLXPLR.Instance.DefaultVertexShader);
            ShaderModule fragment_shader = CreateShaderModule((byte[])GLSLXPLR.Instance.DefaultFragmentShader);

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating shader stage information."));

            PipelineShaderStageCreateInfo vertex_shader_stage_create_info = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageVertexBit,
                PName = (byte*)SilkMarshal.MarshalStringToPtr("main"),
                Module = vertex_shader,
                PSpecializationInfo = null
            };

            PipelineShaderStageCreateInfo fragment_shader_stage_create_info = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.ShaderStageFragmentBit,
                PName = (byte*)SilkMarshal.MarshalStringToPtr("main"),
                Module = fragment_shader,
                PSpecializationInfo = null
            };

            PipelineShaderStageCreateInfo* shader_stages = stackalloc[]
            {
                vertex_shader_stage_create_info,
                fragment_shader_stage_create_info
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating vertex stage information."));

            PipelineVertexInputStateCreateInfo vertex_input_state_create_info = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexAttributeDescriptionCount = 0,
                PVertexAttributeDescriptions = null,
                VertexBindingDescriptionCount = 0,
                PVertexBindingDescriptions = null
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating assembly stage information."));

            PipelineInputAssemblyStateCreateInfo assembly_state_create_info = new PipelineInputAssemblyStateCreateInfo
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

            PipelineViewportStateCreateInfo viewport_state_create_info = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating rasterization information."));

            PipelineRasterizationStateCreateInfo rasterization_state_create_info = new PipelineRasterizationStateCreateInfo
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

            PipelineMultisampleStateCreateInfo multisample_state_create_info = new PipelineMultisampleStateCreateInfo
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

            PipelineColorBlendAttachmentState color_blend_attachment_state = new PipelineColorBlendAttachmentState
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

            PipelineColorBlendStateCreateInfo color_blend_state_create_info = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = Vk.False,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &color_blend_attachment_state
            };

            color_blend_state_create_info.BlendConstants[0] =
                color_blend_state_create_info.BlendConstants[1] =
                    color_blend_state_create_info.BlendConstants[2] =
                        color_blend_state_create_info.BlendConstants[3] = 0.0f;

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "configuring dynamic state."));

            DynamicState* dynamic_states = stackalloc[]
            {
                DynamicState.Viewport
            };

            PipelineDynamicStateCreateInfo dynamic_state_create_info = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 1,
                PDynamicStates = dynamic_states
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating pipeline layout information."));

            PipelineLayoutCreateInfo pipeline_layout_create_info = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 0,
                PSetLayouts = null,
                PushConstantRangeCount = 0,
                PPushConstantRanges = null
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "assigning pipeline layout."));

            fixed (PipelineLayout* pipeline_layout_fixed = &_PipelineLayout)
            {
                if (VK.CreatePipelineLayout(_LogicalDevice, &pipeline_layout_create_info, (AllocationCallbacks*)null!, pipeline_layout_fixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create pipeline layout.");
                }
            }

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "creating final graphics pipeline."));

            GraphicsPipelineCreateInfo graphics_pipeline_create_info = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shader_stages,
                PVertexInputState = &vertex_input_state_create_info,
                PInputAssemblyState = &assembly_state_create_info,
                PViewportState = &viewport_state_create_info,
                PRasterizationState = &rasterization_state_create_info,
                PMultisampleState = &multisample_state_create_info,
                PDepthStencilState = null,
                PColorBlendState = &color_blend_state_create_info,
                PDynamicState = null,
                Layout = _PipelineLayout,
                RenderPass = _RenderPass,
                Subpass = 0,
                BasePipelineHandle = default,
                BasePipelineIndex = -1
            };

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "assigning graphics pipeline."));

            fixed (Pipeline* graphics_pipeline_fixed = &_GraphicsPipeline)
            {
                if (VK.CreateGraphicsPipelines(_LogicalDevice, default, 1, &graphics_pipeline_create_info, (AllocationCallbacks*)null!,
                        graphics_pipeline_fixed)
                    != Result.Success)
                {
                    throw new Exception("Failed to create graphics pipeline.");
                }
            }

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "destroying shader modules."));

            VK.DestroyShaderModule(_LogicalDevice, vertex_shader, (AllocationCallbacks*)null!);
            VK.DestroyShaderModule(_LogicalDevice, fragment_shader, (AllocationCallbacks*)null!);

            Log.Information(string.Format(_VulkanGraphicsPipelineCreationFormat, "-success-"));
        }

        private unsafe ShaderModule CreateShaderModule(byte[] byteCode)
        {
            ShaderModuleCreateInfo shader_module_create_info = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (UIntPtr)byteCode.Length
            };

            fixed (byte* byte_code_fixed = byteCode)
            {
                shader_module_create_info.PCode = (uint*)byte_code_fixed;
            }

            ShaderModule shader_module;

            if (VK.CreateShaderModule(_LogicalDevice, &shader_module_create_info, (AllocationCallbacks*)null!, &shader_module) != Result.Success)
            {
                throw new Exception("Failed to create shader module.");
            }

            return shader_module;
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

                FramebufferCreateInfo framebuffer_create_info = new FramebufferCreateInfo
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

                fixed (Framebuffer* swap_chain_framebuffers_fixed = _SwapChainFramebuffers)
                {
                    if (VK.CreateFramebuffer(_LogicalDevice, &framebuffer_create_info, (AllocationCallbacks*)null!, &swap_chain_framebuffers_fixed[index])
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

            CommandPoolCreateInfo command_pool_create_info = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = _QueueFamilyIndices.GraphicsFamily.Value,
                Flags = 0
            };

            Log.Information(string.Format(_VulkanCommandPoolCreationFormat, "assigning command pool."));

            fixed (CommandPool* command_pool_fixed = &_CommandPool)
            {
                if (VK.CreateCommandPool(_LogicalDevice, &command_pool_create_info, (AllocationCallbacks*)null!, command_pool_fixed) != Result.Success)
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

            CommandBufferAllocateInfo command_buffer_allocate_info = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _CommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)_CommandBuffers.Length
            };

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "assigning command buffers."));

            fixed (CommandBuffer* command_buffers_fixed = _CommandBuffers)
            {
                if (VK.AllocateCommandBuffers(_LogicalDevice, &command_buffer_allocate_info, command_buffers_fixed) != Result.Success)
                {
                    throw new Exception("Failed to create command buffers.");
                }
            }

            Log.Information(string.Format(_VulkanCommandBuffersCreationFormat, "configuring command buffers."));

            for (int index = 0; index < _CommandBuffers.Length; index++)
            {
                CommandBufferBeginInfo command_buffer_begin_info = new CommandBufferBeginInfo
                {
                    SType = StructureType.CommandBufferBeginInfo,
                    Flags = 0,
                    PInheritanceInfo = null
                };

                if (VK.BeginCommandBuffer(_CommandBuffers[index], &command_buffer_begin_info) != Result.Success)
                {
                    throw new Exception("Failed to begin recording command buffer.");
                }

                ClearValue clear_value = new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f));

                RenderPassBeginInfo render_pass_begin_info = new RenderPassBeginInfo
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = _RenderPass,
                    Framebuffer = _SwapChainFramebuffers[index],
                    RenderArea = new Rect2D(new Offset2D(0), _SwapChainExtents),
                    ClearValueCount = 1,
                    PClearValues = &clear_value
                };

                VK.CmdBeginRenderPass(_CommandBuffers[index], &render_pass_begin_info, SubpassContents.Inline);
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

            SemaphoreCreateInfo semaphore_create_info = new SemaphoreCreateInfo
            {
                SType = StructureType.SemaphoreCreateInfo
            };

            _ImageAvailableSemaphores = new Semaphore[_MAX_FRAMES_IN_FLIGHT];
            _RenderFinishedSemaphores = new Semaphore[_MAX_FRAMES_IN_FLIGHT];

            fixed (Semaphore* image_available_semaphores_fixed = _ImageAvailableSemaphores)
            {
                fixed (Semaphore* render_finished_semaphores_fixed = _RenderFinishedSemaphores)
                {
                    for (int index = 0; index < _MAX_FRAMES_IN_FLIGHT; index++)
                    {
                        if (VK.CreateSemaphore(_LogicalDevice, &semaphore_create_info, (AllocationCallbacks*)null!,
                                &image_available_semaphores_fixed[index])
                            != Result.Success)
                        {
                            throw new Exception("Failed to create image availability semaphore.");
                        }

                        if (VK.CreateSemaphore(_LogicalDevice, &semaphore_create_info, (AllocationCallbacks*)null!,
                                &render_finished_semaphores_fixed[index])
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

            foreach (ImageView image_view in _SwapChainImageViews)
            {
                VK.DestroyImageView(_LogicalDevice, image_view, (AllocationCallbacks*)null!);
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
