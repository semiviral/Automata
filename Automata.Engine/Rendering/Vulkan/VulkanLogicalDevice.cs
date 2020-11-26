using System.Diagnostics;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanLogicalDevice : VulkanObject
    {
        private readonly Device _LogicalDevice;
        private Queue _GraphicsQueue;
        private Queue _PresentationQueue;

        public override nint Handle => _LogicalDevice.Handle;

        internal unsafe VulkanLogicalDevice(Vk vk, VulkanPhysicalDevice physicalDevice, string[] extensions, string[]? validationLayers) : base(vk)
        {
            float queuePriority = 1f;
            QueueFamilyIndices queueFamilyIndices = physicalDevice.GetQueueFamilies();
            uint queueFamiliesCount = queueFamilyIndices.GetLength;
            DeviceQueueCreateInfo* deviceQueueCreateInfos = stackalloc DeviceQueueCreateInfo[2];

            for (int i = 0; i < queueFamiliesCount; i++)
            {
                Debug.Assert(queueFamilyIndices[i] != null);

                deviceQueueCreateInfos[i] = new DeviceQueueCreateInfo
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = queueFamilyIndices[i]!.Value,
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority
                };
            }

            nint extensionsPointer = SilkMarshal.StringArrayToPtr(extensions);
            nint? validationLayersPointer = null;

            DeviceCreateInfo deviceCreateInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                EnabledExtensionCount = (uint)extensions.Length,
                PpEnabledExtensionNames = (byte**)extensionsPointer,
                QueueCreateInfoCount = queueFamiliesCount,
                PQueueCreateInfos = deviceQueueCreateInfos,
                PEnabledFeatures = (PhysicalDeviceFeatures*)null!,
                EnabledLayerCount = 0,
                PpEnabledLayerNames = null
            };

            if (validationLayers is not null)
            {
                validationLayersPointer = SilkMarshal.StringArrayToPtr(validationLayers);
                deviceCreateInfo.EnabledLayerCount = (uint)validationLayers.Length;
                deviceCreateInfo.PpEnabledLayerNames = (byte**)validationLayersPointer.Value;
            }

            Result result = VK.CreateDevice(physicalDevice, &deviceCreateInfo, (AllocationCallbacks*)null!, out _LogicalDevice);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create logical device.");
            }

            VK.GetDeviceQueue(this, queueFamilyIndices.GraphicsFamily!.Value, 0, out _GraphicsQueue);
            VK.GetDeviceQueue(this, queueFamilyIndices.PresentationFamily!.Value, 0, out _PresentationQueue);

            SilkMarshal.Free(extensionsPointer);

            if (validationLayersPointer is not null)
            {
                SilkMarshal.Free(validationLayersPointer.Value);
            }
        }


        #region Conversions

        public static implicit operator Device(VulkanLogicalDevice logicalDevice) => logicalDevice._LogicalDevice;

        #endregion
    }
}
