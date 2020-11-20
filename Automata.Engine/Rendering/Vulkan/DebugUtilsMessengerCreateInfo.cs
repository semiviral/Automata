using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public unsafe readonly struct DebugUtilsMessengerCreateInfo
    {
        /// <summary></summary>
        public readonly StructureType SType;

        /// <summary></summary>
        public readonly void* PNext;

        /// <summary></summary>
        public readonly uint Flags;

        /// <summary></summary>
        public readonly DebugUtilsMessageSeverityFlagsEXT MessageSeverity;

        /// <summary></summary>
        public readonly DebugUtilsMessageTypeFlagsEXT MessageType;

        /// <summary></summary>
        public readonly delegate*<
            DebugUtilsMessageSeverityFlagsEXT,
            DebugUtilsMessageTypeFlagsEXT,
            DebugUtilsMessengerCallbackDataEXT*,
            void*, uint> DebugUtilsCallback;

        /// <summary></summary>
        public readonly void* PUserData;

        public DebugUtilsMessengerCreateInfo(
            StructureType sType = StructureType.DebugUtilsMessengerCreateInfoExt,
            void* pNext = null,
            uint flags = 0,
            DebugUtilsMessageSeverityFlagsEXT messageSeverity = (DebugUtilsMessageSeverityFlagsEXT)0,
            DebugUtilsMessageTypeFlagsEXT messageType = (DebugUtilsMessageTypeFlagsEXT)0,
            delegate*<DebugUtilsMessageSeverityFlagsEXT,
                DebugUtilsMessageTypeFlagsEXT,
                DebugUtilsMessengerCallbackDataEXT*,
                void*, uint> debugUtilsCallback = null,
            void* pUserData = null)
        {
            SType = sType;
            PNext = pNext;
            Flags = flags;
            MessageSeverity = messageSeverity;
            MessageType = messageType;
            DebugUtilsCallback = debugUtilsCallback;
            PUserData = pUserData;
        }


        #region Conversions

        public static implicit operator DebugUtilsMessengerCreateInfoEXT(DebugUtilsMessengerCreateInfo createInfo) =>
            Unsafe.As<DebugUtilsMessengerCreateInfo, DebugUtilsMessengerCreateInfoEXT>(ref createInfo);

        #endregion
    }
}
