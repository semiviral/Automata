#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Automata.GLFW;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

#endregion

namespace Automata.Rendering.Vulkan
{
    public class VKAPI : Singleton<VKAPI>
    {
#if DEBUG
        private const bool _ENABLE_VULKAN_VALIDATION = true;
#else
        private const bool _ENABLE_VULKAN_VALIDATION = false;
#endif

        private string[] _ValidationLayers =
        {
            "VK_LAYER_KHRONOS_validation"
        };

        private string[] _InstanceExtensions =
        {
            ExtDebugUtils.ExtensionName
        };

        private Instance _VKInstance;
        private DebugUtilsMessengerEXT _DebugMessenger;
        private ExtDebugUtils _ExtDebugUtils;

        public Vk VK { get; }

        public Instance VKInstance => _VKInstance;

        public VKAPI()
        {
            AssignSingletonInstance(this);
            VK = Vk.GetApi();
        }

        #region Vulkan Initialization

        public unsafe void CreateVulkanInstance()
        {
            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: -begin-");

            if (_ENABLE_VULKAN_VALIDATION && !CheckValidationLayerSupport())
            {
                throw new NotSupportedException($"Validation layers specified in '{nameof(_ValidationLayers)}' not present.");
            }

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: building application info.");

            ApplicationInfo applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Automata"),
                ApplicationVersion = new Version32(0, 0, 1),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("No Engine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version12
            };

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: building instance creation info.");

            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo
            };


            // get required extensions
            byte** requiredExtensions = (byte**)AutomataWindow.Instance.Surface.GetRequiredExtensions(out uint extensionCount);

            // create array to hold final extensions list (required by glfw + required by instance)
            byte** aggregateExtensions = stackalloc byte*[(int)(extensionCount + _InstanceExtensions.Length)];

            // copy glfw required extensions
            for (int index = 0; index < extensionCount; index++)
            {
                aggregateExtensions[index] = requiredExtensions[index];
            }

            // copy instance required extensions
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
            }
            else
            {
                instanceCreateInfo.EnabledLayerCount = 0;
                instanceCreateInfo.PpEnabledLayerNames = null;
            }

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: creating instance.");

            fixed (Instance* instance = &_VKInstance)
            {
                if (VK.CreateInstance(&instanceCreateInfo, null, instance) != Result.Success)
                {
                    throw new Exception("Failed to create Vulkan instance.");
                }
            }

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: applying instance.");

            VK.CurrentInstance = _VKInstance;

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: freeing unmanaged memory.");

            Marshal.FreeHGlobal((IntPtr)applicationInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)applicationInfo.PEngineName);

            if (_ENABLE_VULKAN_VALIDATION)
            {
                SilkMarshal.FreeStringArrayPtr((IntPtr)instanceCreateInfo.PpEnabledLayerNames, _ValidationLayers.Length);
            }

            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: -success-");
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
            Log.Debug($"({nameof(VKAPI)}) Creating Vulkan instance: checking validation layers.");

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

        public unsafe void AttemptEnableDebugMessenger()
        {
            static unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
            {
                static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageType,
                    DebugUtilsMessengerCallbackDataEXT* callbackData, void* userData)
                {
                    string messageFormat = $"({nameof(VKAPI)}: " + "{0}) {1}";

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
                createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt
                                             | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityInfoBitExt
                                             | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt
                                             | DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt;
                createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeGeneralBitExt
                                         | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeValidationBitExt
                                         | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypePerformanceBitExt;
                createInfo.PfnUserCallback = FuncPtr.Of<DebugUtilsMessengerCallbackFunctionEXT>(DebugCallback);
            }

            if (!_ENABLE_VULKAN_VALIDATION || !VK.TryGetExtension(out _ExtDebugUtils))
            {
                return;
            }

            DebugUtilsMessengerCreateInfoEXT createInfo = new DebugUtilsMessengerCreateInfoEXT();
            PopulateDebugMessengerCreateInfo(ref createInfo);

            fixed (DebugUtilsMessengerEXT* debugMessenger = &_DebugMessenger)
            {
                if (_ExtDebugUtils.CreateDebugUtilsMessenger(_VKInstance, &createInfo, null, debugMessenger) != Result.Success)
                {
                    throw new Exception($"Failed to create '{typeof(DebugUtilsMessengerEXT)}'");
                }
            }
        }

        public unsafe void DestroyVulkanInstance()
        {
            if (_ENABLE_VULKAN_VALIDATION)
            {
                _ExtDebugUtils.DestroyDebugUtilsMessenger(VKInstance, _DebugMessenger, null);
            }

            VK.DestroyInstance(VKInstance, null);
        }
    }

    #endregion
}
