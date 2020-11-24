using System;
using System.Runtime.InteropServices;
using Serilog;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanDebugMessenger : VulkanObject
    {
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

        private readonly ExtDebugUtils _DebugUtils;
        private readonly DebugUtilsMessengerEXT _Messenger;

        public unsafe VulkanDebugMessenger(VulkanInstance instance) : base(instance.VK)
        {
            _DebugUtils = instance.GetInstanceExtension<ExtDebugUtils>();

            DebugUtilsMessengerCreateInfoEXT createInfo = new DebugUtilsMessengerCreateInfo(
                StructureType.DebugUtilsMessengerCreateInfoExt,
                null, 0u,
                _MESSAGE_SEVERITY_IMPORTANT,
                DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeGeneralBitExt
                | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeValidationBitExt
                | DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypePerformanceBitExt,
                &DebugCallback
            );

            Result result = _DebugUtils.CreateDebugUtilsMessenger((Instance)instance, &createInfo, (AllocationCallbacks*)null!, out _Messenger);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create debug messenger,");
            }
        }

        private static unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageType,
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
                default: throw new ArgumentOutOfRangeException(nameof(messageSeverity), messageSeverity, null);
            }

            return Vk.False;
        }
    }
}
