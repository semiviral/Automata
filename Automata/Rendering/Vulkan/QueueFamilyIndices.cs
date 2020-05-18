namespace Automata.Rendering.Vulkan
{
    public struct QueueFamilyIndices
    {
        public uint? GraphicsFamily { get; set; }
        public uint? PresentFamily { get; set; }

        public bool IsCompleted() => GraphicsFamily.HasValue; // && PresentFamily.HasValue;
    }
}
