using System;
using Silk.NET.Vulkan;

namespace Automata.Engine.Rendering.Vulkan
{
    public class VulkanException : Exception
    {
        public Result Result { get; }

        public VulkanException(Result result, string? message) : base($"Vulkan exception occurred (result {result}): {message}") => Result = result;
    }
}
