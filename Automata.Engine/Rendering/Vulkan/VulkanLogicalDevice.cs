using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automata.Engine.Rendering.Vulkan.NativeExtensions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanLogicalDevice : VulkanObject
    {
        private readonly VulkanContext _Context;
        private readonly Device _LogicalDevice;
        private Queue _GraphicsQueue;
        private Queue _PresentationQueue;

        public SwapchainExtension SwapchainExtension { get; }

        public override nint Handle => _LogicalDevice.Handle;

        internal unsafe VulkanLogicalDevice(Vk vk, VulkanContext context, string[] extensions, string[]? validationLayers) : base(vk)
        {
            _Context = context;
            SwapchainExtension = GetDeviceExtension<SwapchainExtension>();

            float queuePriority = 1f;
            QueueFamilyIndices queueFamilyIndices = _Context.PhysicalDevice.GetQueueFamilies();
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

            Result result = VK.CreateDevice(_Context.PhysicalDevice, &deviceCreateInfo, (AllocationCallbacks*)null!, out _LogicalDevice);

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

        public VulkanSwapChain CreateSwapChain(
            VulkanSwapChain.ChooseSwapChainSurfaceFormat chooseFormat,
            VulkanSwapChain.ChooseSwapChainPresentMode choosePresentMode,
            VulkanSwapChain.ChooseSwapChainExtents chooseExtents) =>
            new VulkanSwapChain(VK, _Context with { LogicalDevice = this }, chooseFormat, choosePresentMode, chooseExtents);

        public T GetDeviceExtension<T>() where T : NativeExtension<Vk>
        {
            if (TryGetDeviceExtension(out T? extension))
            {
                return extension;
            }
            else
            {
                throw new VulkanException(Result.ErrorExtensionNotPresent, "Could not load extension.");
            }
        }

        public bool TryGetDeviceExtension<T>([NotNullWhen(true)] out T? extension) where T : NativeExtension<Vk> =>
            VK.TryGetDeviceExtension(_Context.Instance!, this, out extension);


        #region Conversions

        public static implicit operator Device(VulkanLogicalDevice logicalDevice) => logicalDevice._LogicalDevice;

        #endregion
    }
}
