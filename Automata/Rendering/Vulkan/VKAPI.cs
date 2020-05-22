#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Devices.Lights.Effects;
using Automata.Rendering.DirectX;
using Automata.Rendering.GLFW;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

#endregion

// ReSharper disable RedundantLogicalConditionalExpressionOperand
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable RedundantCast

namespace Automata.Rendering.Vulkan
{
    public class VKAPI : Singleton<VKAPI>
    {
#if DEBUG
        private const bool _ENABLE_VULKAN_VALIDATION = true;
#else
        private const bool _ENABLE_VULKAN_VALIDATION = false;
#endif

        private const DebugUtilsMessageSeverityFlagsEXT _MESSAGE_SEVERITY_ALL =
            DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityInfoBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt;

        private const DebugUtilsMessageSeverityFlagsEXT _MESSAGE_SEVERITY_GENERAL =
            DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityInfoBitExt;

        private const DebugUtilsMessageSeverityFlagsEXT _MESSAGE_SEVERITY_IMPORTANT =
            DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt
            | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt;

        private static readonly string _VulkanInstanceCreationFormat = $"({nameof(VKAPI)}) Creating instance: {{0}}";
        private static readonly string _VulkanSurfaceCreationFormat = $"({nameof(VKAPI)}) Creating surface: {{0}}";
        private static readonly string _VulkanDebugMessengerCreationFormat = $"({nameof(VKAPI)}) Creating debug messenger: {{0}}";
        private static readonly string _VulkanPhysicalDeviceSelectionFormat = $"({nameof(VKAPI)}) Selecting physical device: {{0}}";
        private static readonly string _VulkanLogicalDeviceCreationFormat = $"({nameof(VKAPI)}) Creating logical device: {{0}}";
        private static readonly string _VulkanSwapChainCreationFormat = $"({nameof(VKAPI)}) Creating swap chain: {{0}}";
        private static readonly string _VulkanImageViewCreationFormat = $"({nameof(VKAPI)}) Creating image views: {{0}}";
        private static readonly string _VulkanRenderPassCreationFormat = $"({nameof(VKAPI)}) Creating render pass: {{0}}";
        private static readonly string _VulkanGraphicsPipelineCreationFormat = $"({nameof(VKAPI)}) Creating graphics pipeline: {{0}}";


        private readonly string[] _ValidationLayers =
        {
            "VK_LAYER_KHRONOS_validation"
        };

        private readonly string[] _InstanceExtensions =
        {
            ExtDebugUtils.ExtensionName
        };

        private readonly string[] _DeviceExtensions =
        {
            KhrSwapchain.ExtensionName
        };

        private Instance _VKInstance;
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

        public Vk VK { get; }

        public Instance VKInstance => _VKInstance;

        public VKAPI()
        {
            AssignSingletonInstance(this);
            VK = Vk.GetApi();
        }

        #region Vulkan Initialization

        public void DefaultInitialize()
        {
            Log.Information($"({nameof(VKAPI)}) Initializing Vulkan: -begin-");

            CreateInstance();
            CreateSurface();
            SetupDebugMessenger();
            SelectPhysicalDevice();
            CreateLogicalDevice();
            CreateSwapChain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();

            Log.Information($"({nameof(VKAPI)}) Initializing Vulkan: -success-");
        }

        #region Create Instance

        private unsafe void CreateInstance()
        {
            Log.Information(string.Format(_VulkanInstanceCreationFormat, "-begin-"));

            if (_ENABLE_VULKAN_VALIDATION && !CheckValidationLayerSupport())
            {
                throw new NotSupportedException($"Validation layers specified in '{nameof(_ValidationLayers)}' not present.");
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "building application info."));

            ApplicationInfo applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Automata"),
                ApplicationVersion = new Version32(0, 0, 1),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("No Engine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version12
            };

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "building instance creation info."));


            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
            };

            byte** requiredExtensions = (byte**)AutomataWindow.Instance.Surface.GetRequiredExtensions(out uint extensionCount);
            byte** aggregateExtensions = stackalloc byte*[(int)(extensionCount + _InstanceExtensions.Length)];

            for (int index = 0; index < extensionCount; index++)
            {
                aggregateExtensions[index] = requiredExtensions[index];
            }

            for (int index = 0; index < _InstanceExtensions.Length; index++)
            {
                aggregateExtensions[extensionCount + index] = (byte*)SilkMarshal.MarshalStringToPtr(_InstanceExtensions[index]);
            }

            extensionCount += (uint)_InstanceExtensions.Length;

            instanceCreateInfo.EnabledExtensionCount = extensionCount;
            instanceCreateInfo.PpEnabledExtensionNames = aggregateExtensions;

            if (_ENABLE_VULKAN_VALIDATION)
            {
                instanceCreateInfo.EnabledLayerCount = (uint)_ValidationLayers.Length;
                instanceCreateInfo.PpEnabledLayerNames = (byte**)SilkMarshal.MarshalStringArrayToPtr(_ValidationLayers);

                Log.Information(string.Format(_VulkanInstanceCreationFormat, "creating debug instance info."));

                DebugUtilsMessengerCreateInfoEXT debugMessengerCreationInfo = new DebugUtilsMessengerCreateInfoEXT();
                PopulateDebugMessengerCreateInfo(ref debugMessengerCreationInfo, _MESSAGE_SEVERITY_IMPORTANT);
                instanceCreateInfo.PNext = &debugMessengerCreationInfo;
            }
            else
            {
                instanceCreateInfo.EnabledLayerCount = 0;
                instanceCreateInfo.PpEnabledLayerNames = null;
                instanceCreateInfo.PNext = null;
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "creating instance."));

            fixed (Instance* vkInstance = &_VKInstance)
            {
                if (VK.CreateInstance(&instanceCreateInfo, null, vkInstance) != Result.Success)
                {
                    throw new Exception("Failed to create Vulkan instance.");
                }
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "assigning Vulkan instance."));

            VK.CurrentInstance = VKInstance;

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "querying surface extension."));

            if (!VK.TryGetExtension(out _KHRSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "querying swapchain extension."));

            if (!VK.TryGetExtension(out _KHRSwapChain))
            {
                throw new NotSupportedException("KHR_swapchain extension not found.");
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "freeing unmanaged memory."));

            Marshal.FreeHGlobal((IntPtr)applicationInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)applicationInfo.PEngineName);

            if (_ENABLE_VULKAN_VALIDATION)
            {
                SilkMarshal.FreeStringArrayPtr((IntPtr)instanceCreateInfo.PpEnabledLayerNames, _ValidationLayers.Length);
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "-success-"));
        }

        private unsafe bool CheckValidationLayerSupport()
        {
            Log.Information(string.Format(_VulkanInstanceCreationFormat, "checking validation layers."));

            uint layerCount;
            VK.EnumerateInstanceLayerProperties(&layerCount, (LayerProperties*)null!);

            LayerProperties[] layerProperties = new LayerProperties[layerCount];
            VK.EnumerateInstanceLayerProperties(ref layerCount, ref layerProperties[0]);

            foreach (string validationLayer in _ValidationLayers)
            {
                bool layerFound = layerProperties.Any(layerProperty =>
                    validationLayer == Marshal.PtrToStringAnsi((IntPtr)layerProperty.LayerName));

                if (!layerFound)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Create Surface

        private unsafe void CreateSurface()
        {
            Log.Information(string.Format(_VulkanSurfaceCreationFormat, $"retrieving surface from '{nameof(AutomataWindow)}'"));

            _Surface = AutomataWindow.Instance.Surface.Create(VKInstance.ToHandle(), (AllocationCallbacks*)null!).ToSurface();
        }

        #endregion

        #region Debug Messenger

        private unsafe void SetupDebugMessenger()
        {
            Log.Information(string.Format(_VulkanDebugMessengerCreationFormat, "-begin-"));

            if (!_ENABLE_VULKAN_VALIDATION || !VK.TryGetExtension(out _ExtDebugUtils))
            {
                throw new Exception("Failed to get extension for debug messenger.");
            }

            Log.Information(string.Format(_VulkanDebugMessengerCreationFormat, "initializing creation info."));

            DebugUtilsMessengerCreateInfoEXT createInfo = new DebugUtilsMessengerCreateInfoEXT();
            PopulateDebugMessengerCreateInfo(ref createInfo, _MESSAGE_SEVERITY_IMPORTANT);

            Log.Information(string.Format(_VulkanDebugMessengerCreationFormat, "assigning debug messenger."));


            fixed (DebugUtilsMessengerEXT* debugMessenger = &_DebugMessenger)
            {
                if (_ExtDebugUtils.CreateDebugUtilsMessenger(VKInstance, &createInfo, (AllocationCallbacks*)null!, debugMessenger)
                    != Result.Success)
                {
                    throw new Exception($"Failed to create '{typeof(DebugUtilsMessengerEXT)}'");
                }
            }

            Log.Information(string.Format(_VulkanDebugMessengerCreationFormat, "-success-"));
        }

        private static unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo,
            DebugUtilsMessageSeverityFlagsEXT messageSeverityFlags)
        {
            static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageType,
                DebugUtilsMessengerCallbackDataEXT* callbackData, void* userData)
            {
                string messageFormat = $"({nameof(VKAPI)}) {{0}}: {{1}}";

                switch (messageSeverity)
                {
                    case DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt:
                        Log.Verbose(string.Format(messageFormat, messageType, Marshal.PtrToStringAnsi((IntPtr)callbackData->PMessage)));
                        break;
                    case DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityInfoBitExt:
                        Log.Information(string.Format(messageFormat, messageType, Marshal.PtrToStringAnsi((IntPtr)callbackData->PMessage)));
                        break;
                    case DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt:
                        Log.Warning(string.Format(messageFormat, messageType, Marshal.PtrToStringAnsi((IntPtr)callbackData->PMessage)));
                        break;
                    case DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt:
                        Log.Error(string.Format(messageFormat, messageType, Marshal.PtrToStringAnsi((IntPtr)callbackData->PMessage)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageSeverity), messageSeverity, null);
                }

                return Vk.False;
            }

            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = messageSeverityFlags;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeGeneralBitExt
                                     | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeValidationBitExt
                                     | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypePerformanceBitExt;
            createInfo.PfnUserCallback = FuncPtr.Of<DebugUtilsMessengerCallbackFunctionEXT>(DebugCallback);
        }

        #endregion

        #region Physical Device Selection

        private unsafe void SelectPhysicalDevice()
        {
            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, "-begin-"));

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, "locating all GPUs."));

            uint deviceCount = 0u;
            VK.EnumeratePhysicalDevices(VKInstance, &deviceCount, (PhysicalDevice*)null!);

            if (deviceCount == 0u)
            {
                throw new Exception("No GPUs found with Vulkan support.");
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"{deviceCount} GPUs found."));

            PhysicalDevice[] devices = new PhysicalDevice[deviceCount];
            VK.EnumeratePhysicalDevices(VKInstance, ref deviceCount, ref devices[0]);

            for (int index = 0; index < deviceCount; index++)
            {
                if (IsDeviceSuitable(devices[index], out string gpuName, out QueueFamilyIndices queueFamilyIndices))
                {
                    Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}'."));

                    _PhysicalDevice = devices[index];
                    _QueueFamilyIndices = queueFamilyIndices;

                    Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, "-success-"));

                    return;
                }
                else
                {
                    Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"'{gpuName}' failed."));
                }
            }

            throw new Exception("No suitable GPUs found.");
        }

        private unsafe bool IsDeviceSuitable(PhysicalDevice physicalDevice, out string gpuName, out QueueFamilyIndices queueFamilyIndices)
        {
            gpuName = string.Empty;
            queueFamilyIndices = default;

            VK.GetPhysicalDeviceProperties(physicalDevice, out PhysicalDeviceProperties physicalDeviceProperties);
            gpuName = SilkMarshal.MarshalPtrToString((IntPtr)physicalDeviceProperties.DeviceName);

            if (physicalDeviceProperties.DeviceType != PhysicalDeviceType.DiscreteGpu)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' device type."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' device type."));


            if (!CheckDeviceExtensionSupport(physicalDevice))
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat,
                    $"failed to verify '{gpuName}' extension support ({string.Join(", ", _DeviceExtensions)})."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat,
                $"verified '{gpuName}' extensions support ({string.Join(", ", _DeviceExtensions)})."));


            _SwapChainSupportDetails = GetSwapChainSupport(physicalDevice);

            if (_SwapChainSupportDetails.Formats.Length == 0)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' swap chain formats."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' swap chain formats."));

            if (_SwapChainSupportDetails.PresentModes.Length == 0)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' swap chain presentation modes."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' swap chain presentation modes."));


            queueFamilyIndices = FindQueueFamilies(physicalDevice);

            if (!queueFamilyIndices.IsCompleted())
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' queue families."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' queue families."));


            VK.GetPhysicalDeviceFeatures(physicalDevice, out PhysicalDeviceFeatures physicalDeviceFeatures);

            if (!physicalDeviceFeatures.GeometryShader)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' geometry shader support."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' geometry shader support."));

            return true;
        }

        private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice physicalDevice)
        {
            uint extensionCount = 0u;
            VK.EnumerateDeviceExtensionProperties(physicalDevice, string.Empty, &extensionCount, (ExtensionProperties*)null!);

            ExtensionProperties* extensionProperties = stackalloc ExtensionProperties[(int)extensionCount];
            VK.EnumerateDeviceExtensionProperties(physicalDevice, string.Empty, &extensionCount, extensionProperties);

            List<string?> requiredExtensions = new List<string?>(_DeviceExtensions);

            for (uint index = 0u; index < extensionCount; index++)
            {
                requiredExtensions.Remove(Marshal.PtrToStringAnsi((IntPtr)extensionProperties[index].ExtensionName));
            }

            return requiredExtensions.Count == 0;
        }

        private unsafe SwapChainSupportDetails GetSwapChainSupport(PhysicalDevice physicalDevice)
        {
            SurfaceCapabilitiesKHR surfaceCapabilities;
            _KHRSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, _Surface, &surfaceCapabilities);

            SwapChainSupportDetails swapChainSupportDetails = new SwapChainSupportDetails
            {
                SurfaceCapabilities = surfaceCapabilities
            };

            uint surfaceFormatsCount = 0u;
            _KHRSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _Surface, &surfaceFormatsCount, (SurfaceFormatKHR*)null!);

            if (surfaceFormatsCount != 0u)
            {
                SurfaceFormatKHR* surfaceFormats = stackalloc SurfaceFormatKHR[(int)surfaceFormatsCount];
                _KHRSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _Surface, &surfaceFormatsCount, surfaceFormats);

                swapChainSupportDetails.Formats = new SurfaceFormatKHR[(int)surfaceFormatsCount];
                for (int index = 0; index < surfaceFormatsCount; index++)
                {
                    swapChainSupportDetails.Formats[index] = surfaceFormats[index];
                }
            }
            else
            {
                throw new NotSupportedException("This device does not support surface formats.");
            }

            uint presentationModeCount;
            _KHRSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, _Surface, &presentationModeCount, (PresentModeKHR*)null!);

            if (presentationModeCount != 0u)
            {
                PresentModeKHR* presentModes = stackalloc PresentModeKHR[(int)presentationModeCount];
                _KHRSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, _Surface, &presentationModeCount, presentModes);

                swapChainSupportDetails.PresentModes = new PresentModeKHR[(int)presentationModeCount];
                for (int index = 0; index < presentationModeCount; index++)
                {
                    swapChainSupportDetails.PresentModes[index] = presentModes[index];
                }
            }
            else
            {
                throw new NotSupportedException("Device does not support presentation formats.");
            }

            return swapChainSupportDetails;
        }

        private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice physicalDevice)
        {
            QueueFamilyIndices indices = new QueueFamilyIndices();

            uint queueFamilyPropertiesCount = 0u;
            VK.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertiesCount, null);

            QueueFamilyProperties* queueFamilyProperties = stackalloc QueueFamilyProperties[(int)queueFamilyPropertiesCount];
            VK.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertiesCount, queueFamilyProperties);

            for (uint index = 0; index < queueFamilyPropertiesCount; index++)
            {
                QueueFamilyProperties queueFamily = queueFamilyProperties[index];

                if (queueFamily.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    indices.GraphicsFamily = index;
                }

                _KHRSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, index, _Surface, out Bool32 presentationSupport);

                if (presentationSupport == Vk.True)
                {
                    indices.PresentationFamily = index;
                }

                if (indices.IsCompleted())
                {
                    break;
                }
            }

            return indices;
        }

        #endregion

        #region Create Logical Device

        private unsafe void CreateLogicalDevice()
        {
            float queuePriority = 1f;

            Debug.Assert(_QueueFamilyIndices.GraphicsFamily != null);

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "-begin-"));

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "initializing device queue creation info."));

            uint queueFamiliesCount = _QueueFamilyIndices.GetLength;
            DeviceQueueCreateInfo* deviceQueueCreateInfos = stackalloc DeviceQueueCreateInfo[2];

            for (int i = 0; i < queueFamiliesCount; i++)
            {
                Debug.Assert(_QueueFamilyIndices[i] != null);

                deviceQueueCreateInfos[i] = new DeviceQueueCreateInfo
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = _QueueFamilyIndices[i].Value,
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority,
                };
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "initializing device creation info."));

            PhysicalDeviceFeatures physicalDeviceFeatures = new PhysicalDeviceFeatures();
            DeviceCreateInfo deviceCreateInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                EnabledExtensionCount = (uint)_DeviceExtensions.Length,
                PpEnabledExtensionNames = (byte**)SilkMarshal.MarshalStringArrayToPtr(_DeviceExtensions),
                QueueCreateInfoCount = queueFamiliesCount,
                PQueueCreateInfos = deviceQueueCreateInfos,
                PEnabledFeatures = &physicalDeviceFeatures
            };

            if (_ENABLE_VULKAN_VALIDATION)
            {
                deviceCreateInfo.EnabledLayerCount = (uint)_ValidationLayers.Length;
                deviceCreateInfo.PpEnabledLayerNames = (byte**)SilkMarshal.MarshalStringArrayToPtr(_ValidationLayers);
            }
            else
            {
                deviceCreateInfo.EnabledLayerCount = 0;
                deviceCreateInfo.PpEnabledLayerNames = null;
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning logical device."));

            fixed (Device* logicalDevice = &_LogicalDevice)
            {
                if (VK.CreateDevice(_PhysicalDevice, &deviceCreateInfo, (AllocationCallbacks*)null!, logicalDevice)
                    != Result.Success)
                {
                    throw new Exception("Failed to create logical device.");
                }
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning graphics queue."));


            Debug.Assert(_QueueFamilyIndices.GraphicsFamily != null);

            fixed (Queue* graphicsQueueFixed = &_GraphicsQueue)
            {
                VK.GetDeviceQueue(_LogicalDevice, _QueueFamilyIndices.GraphicsFamily.Value, 0, graphicsQueueFixed);
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning presentation queue."));

            Debug.Assert(_QueueFamilyIndices.PresentationFamily != null);

            fixed (Queue* presentationQueueFixed = &_PresentationQueue)
            {
                VK.GetDeviceQueue(_LogicalDevice, _QueueFamilyIndices.PresentationFamily.Value, 0, presentationQueueFixed);
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "-success-"));
        }

        #endregion

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
                StencilStoreOp = AttachmentStoreOp.DontCare,InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr
            };

            AttachmentReference subpassAttachmentReference = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            SubpassDescription subpassDescription = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &subpassAttachmentReference
            };

            Log.Information(string.Format(_VulkanRenderPassCreationFormat, "creating render pass information."));

            RenderPassCreateInfo renderPassCreateInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpassDescription
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
                Offset = new Offset2D(0, 0),
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
                DepthBiasSlopeFactor = 0f,
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
                BlendEnable                = Vk.False,
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
                PAttachments = &colorBlendAttachmentState,
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
                CodeSize = (UIntPtr)byteCode.Length,
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

        public unsafe void DestroyVulkanInstance()
        {
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

        ~VKAPI()
        {
            DestroyVulkanInstance();
        }
    }

    #endregion
}
