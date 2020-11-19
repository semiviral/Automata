using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.Vulkan
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QueueFamilyIndices
    {
        public uint? GraphicsFamily { get; set; }
        public uint? PresentationFamily { get; set; }

        public uint? this[int index] => index switch
        {
            0 => GraphicsFamily,
            1 => PresentationFamily,
            _ => throw new IndexOutOfRangeException(nameof(index))
        };

        public bool IsCompleted() => GraphicsFamily.HasValue && PresentationFamily.HasValue;
        public uint GetLength => 0u + (GraphicsFamily.HasValue ? 1u : 0u);
    }
}
