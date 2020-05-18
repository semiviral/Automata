using System;
using System.Runtime.InteropServices;
using Automata.GLFW;
using Automata.Rendering.OpenGL;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;

namespace Automata.Rendering.Vulkan
{
    public class VKAPI : Singleton<VKAPI>
    {
        private Instance _VKInstance;

        public Vk VK { get; }
        public Instance VKInstance => _VKInstance;

        public VKAPI()
        {
            AssignSingletonInstance(this);
            VK = Vk.GetApi();
        }

        public unsafe void CreateVulkanInstance()
        {
            ApplicationInfo applicationInfo = GetApplicationInfo();
            InstanceCreateInfo instanceCreateInfo = GetInstanceCreateInfo(applicationInfo);

            uint extensionCount = 0;

            VK.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, null);
            ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionCount];

            fixed (ExtensionProperties* extensionPropertiesFixed = &extensionProperties[0])
            {
                VK.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, extensionPropertiesFixed);
            }

            fixed (Instance* instance = &_VKInstance)
            {
                if (VK.CreateInstance(&instanceCreateInfo, null, instance) != Result.Success)
                {
                    throw new Exception("Failed to create Vulkan instance.");
                }
            }
        }

        public unsafe void DestroyVulkanInstance()
        {
            VK.DestroyInstance(VKInstance, null);
        }

        private static unsafe ApplicationInfo GetApplicationInfo() =>
            new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Automata"),
                ApplicationVersion = new Version32(0, 0, 1),
                PEngineName =(byte*) Marshal.StringToHGlobalAnsi("No Engine"),
                EngineVersion = new Version32(0, 0, 0),
                ApiVersion = Vk.Version12
            };

        private static unsafe InstanceCreateInfo GetInstanceCreateInfo(ApplicationInfo applicationInfo)
        {
            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                EnabledLayerCount = 0
            };

            byte** extensions = (byte**)AutomataWindow.Instance.Surface.GetRequiredExtensions(out uint extensionCount);
            instanceCreateInfo.EnabledExtensionCount = extensionCount;
            instanceCreateInfo.PpEnabledExtensionNames = extensions;

            return instanceCreateInfo;
        }
    }
}
