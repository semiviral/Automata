#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Automata.Rendering.GLFW;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

#endregion

// ReSharper disable HeuristicUnreachableCode


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
        private static readonly string _VulkanPhysicalDeviceSelectionFormat = $"({nameof(VKAPI)}) Selecting GPU: {{0}}";
        private static readonly string _VulkanLogicalDeviceCreationFormat = $"({nameof(VKAPI)}) Creating logical device: {{0}}.";


        private readonly string[] _ValidationLayers =
        {
            "VK_LAYER_KHRONOS_validation"
        };

        private readonly string[] _InstanceExtensions =
        {
            ExtDebugUtils.ExtensionName
        };

        private Instance _VKInstance;
        private KhrSurface _KHRSurface;
        private SurfaceKHR _Surface;

        private DebugUtilsMessengerEXT _DebugMessenger;
        private ExtDebugUtils _ExtDebugUtils;

        private PhysicalDevice _PhysicalDevice;
        private QueueFamilyIndices _QueueFamilyIndices;
        private Device _LogicalDevice;

        private Queue _GraphicsQueue;
        private Queue _PresentationQueue;

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

            fixed (Instance* instance = &_VKInstance)
            {
                if (VK.CreateInstance(&instanceCreateInfo, null, instance) != Result.Success)
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


            Log.Information(string.Format(_VulkanInstanceCreationFormat, "freeing unmanaged memory."));

            Marshal.FreeHGlobal((IntPtr)applicationInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)applicationInfo.PEngineName);

            if (_ENABLE_VULKAN_VALIDATION)
            {
                SilkMarshal.FreeStringArrayPtr((IntPtr)instanceCreateInfo.PpEnabledLayerNames, _ValidationLayers.Length);
            }

            Log.Information(string.Format(_VulkanInstanceCreationFormat, "-success-"));
        }

        private unsafe IEnumerable<ExtensionProperties> GetExtensionProperties()
        {
            uint extensionCount = 0;

            VK.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, null);
            ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionCount];

            fixed (ExtensionProperties* extensionPropertiesFixed = &extensionProperties[0])
            {
                VK.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, extensionPropertiesFixed);
            }

            return extensionProperties;
        }

        private unsafe bool CheckValidationLayerSupport()
        {
            Log.Information(string.Format(_VulkanInstanceCreationFormat, "checking validation layers."));

            uint layerCount;
            VK.EnumerateInstanceLayerProperties(&layerCount, null);

            LayerProperties[] layerProperties = new LayerProperties[layerCount];

            fixed (LayerProperties* layerProperty = &layerProperties[0])
            {
                VK.EnumerateInstanceLayerProperties(&layerCount, layerProperty);
            }

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

            _Surface = AutomataWindow.Instance.Surface.Create<AllocationCallbacks>(VKInstance.ToHandle(), null).ToSurface();
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

            Log.Information(string.Format(_VulkanDebugMessengerCreationFormat, "assigning debug messenger instance."));

            fixed (DebugUtilsMessengerEXT* debugMessengerFixed = &_DebugMessenger)
            {
                if (_ExtDebugUtils.CreateDebugUtilsMessenger(VKInstance, &createInfo, null, debugMessengerFixed) != Result.Success)
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

            uint deviceCount = 0;
            VK.EnumeratePhysicalDevices(VKInstance, &deviceCount, null);

            if (deviceCount == 0)
            {
                throw new Exception("No GPUs found with Vulkan support.");
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"{deviceCount} GPUs found."));

            PhysicalDevice[] devices = new PhysicalDevice[deviceCount];

            fixed (PhysicalDevice* devicesFixed = &devices[0])
            {
                VK.EnumeratePhysicalDevices(VKInstance, &deviceCount, devicesFixed);
            }

            for (int index = 0; index < deviceCount; index++)
            {
                if (IsDeviceSuitable(devices[index], out string gpuName, out QueueFamilyIndices queueFamilyIndices))
                {
                    Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"'{gpuName}' verified."));

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

            PhysicalDeviceProperties physicalDeviceProperties;
            VK.GetPhysicalDeviceProperties(physicalDevice, &physicalDeviceProperties);
            gpuName = SilkMarshal.MarshalPtrToString((IntPtr)physicalDeviceProperties.DeviceName);

                if (physicalDeviceProperties.DeviceType != PhysicalDeviceType.DiscreteGpu)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' device type."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' device type."));


            queueFamilyIndices = FindQueueFamilies(physicalDevice);

            if (!queueFamilyIndices.IsCompleted())
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' queue families."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' queue families."));


            PhysicalDeviceFeatures physicalDeviceFeatures;
            VK.GetPhysicalDeviceFeatures(physicalDevice, &physicalDeviceFeatures);

            if (!physicalDeviceFeatures.GeometryShader)
            {
                Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"failed to verify '{gpuName}' geometry shader support."));

                return false;
            }

            Log.Information(string.Format(_VulkanPhysicalDeviceSelectionFormat, $"verified '{gpuName}' geometry shader support."));

            return true;
        }

        private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice physicalDevice)
        {
            QueueFamilyIndices indices = new QueueFamilyIndices();

            uint queueFamilyPropertiesCount = 0;
            VK.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertiesCount, null);

            QueueFamilyProperties[] queueFamilyProperties = new QueueFamilyProperties[queueFamilyPropertiesCount];

            fixed (QueueFamilyProperties* queueFamilyPropertiesFixed = &queueFamilyProperties[0])
            {
                VK.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertiesCount, queueFamilyPropertiesFixed);
            }

            for (uint index = 0; index < queueFamilyPropertiesCount; index++)
            {
                QueueFamilyProperties queueFamily = queueFamilyProperties[index];

                if (queueFamily.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    indices.GraphicsFamily = index;
                }

                Bool32 presentationSupport = false;
                _KHRSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, index, _Surface, &presentationSupport);

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
                EnabledExtensionCount = 0,
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

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning logical device instance."));

            fixed (Device* logicalDeviceFixed = &_LogicalDevice)
            {
                if (VK.CreateDevice(_PhysicalDevice, &deviceCreateInfo, null, logicalDeviceFixed) != Result.Success)
                {
                    throw new Exception("Failed to create logical device.");
                }
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning graphics queue."));

            fixed (Queue* graphicsQueueFixed = &_GraphicsQueue)
            {
                Debug.Assert(_QueueFamilyIndices.GraphicsFamily != null);

                VK.GetDeviceQueue(_LogicalDevice, _QueueFamilyIndices.GraphicsFamily.Value, 0, graphicsQueueFixed);
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "assigning presentation queue."));

            fixed (Queue* presentationQueueFixed = &_PresentationQueue)
            {
                Debug.Assert(_QueueFamilyIndices.PresentationFamily != null);

                VK.GetDeviceQueue(_LogicalDevice, _QueueFamilyIndices.PresentationFamily.Value, 0, presentationQueueFixed);
            }

            Log.Information(string.Format(_VulkanLogicalDeviceCreationFormat, "-success-"));
        }

        #endregion

        public unsafe void DestroyVulkanInstance()
        {
            VK.DestroyDevice(_LogicalDevice, null);

            if (_ENABLE_VULKAN_VALIDATION)
            {
                _ExtDebugUtils.DestroyDebugUtilsMessenger(VKInstance, _DebugMessenger, null);
            }

            _KHRSurface.DestroySurface(VKInstance, _Surface, null);
            VK.DestroyInstance(VKInstance, null);
        }
    }

    #endregion
}
