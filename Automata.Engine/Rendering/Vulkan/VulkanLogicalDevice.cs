using System;
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
            if (context.Instance is null || context.PhysicalDevice is null)
            {
                throw new NullReferenceException($"{nameof(VulkanLogicalDevice)} requires a context with a valid instance and physical device.");
            }

            _Context = context;
            SwapchainExtension = GetDeviceExtension<SwapchainExtension>();

            float queue_priority = 1f;
            QueueFamilyIndices queue_family_indices = _Context.PhysicalDevice.GetQueueFamilies();
            uint queue_families_count = queue_family_indices.GetLength;
            DeviceQueueCreateInfo* device_queue_create_infos = stackalloc DeviceQueueCreateInfo[2];

            for (int i = 0; i < queue_families_count; i++)
            {
                Debug.Assert(queue_family_indices[i] != null);

                device_queue_create_infos[i] = new DeviceQueueCreateInfo
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = queue_family_indices[i]!.Value,
                    QueueCount = 1,
                    PQueuePriorities = &queue_priority
                };
            }

            nint extensions_pointer = SilkMarshal.StringArrayToPtr(extensions);
            nint? validation_layers_pointer = null;

            DeviceCreateInfo device_create_info = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                EnabledExtensionCount = (uint)extensions.Length,
                PpEnabledExtensionNames = (byte**)extensions_pointer,
                QueueCreateInfoCount = queue_families_count,
                PQueueCreateInfos = device_queue_create_infos,
                PEnabledFeatures = (PhysicalDeviceFeatures*)null!,
                EnabledLayerCount = 0,
                PpEnabledLayerNames = null
            };

            if (validationLayers is not null)
            {
                validation_layers_pointer = SilkMarshal.StringArrayToPtr(validationLayers);
                device_create_info.EnabledLayerCount = (uint)validationLayers.Length;
                device_create_info.PpEnabledLayerNames = (byte**)validation_layers_pointer.Value;
            }

            Result result = VK.CreateDevice(_Context.PhysicalDevice, &device_create_info, (AllocationCallbacks*)null!, out _LogicalDevice);

            if (result is not Result.Success)
            {
                throw new VulkanException(result, "Failed to create logical device.");
            }

            VK.GetDeviceQueue(this, queue_family_indices.GraphicsFamily!.Value, 0, out _GraphicsQueue);
            VK.GetDeviceQueue(this, queue_family_indices.PresentationFamily!.Value, 0, out _PresentationQueue);

            SilkMarshal.Free(extensions_pointer);

            if (validation_layers_pointer is not null)
            {
                SilkMarshal.Free(validation_layers_pointer.Value);
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
