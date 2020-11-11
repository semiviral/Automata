using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DrawElementsIndirectCommand
    {
        public uint Count { get; init; }
        public uint InstanceCount { get; init; }
        public uint FirstIndexOffset { get; init; }
        public uint BaseVertex { get; init; }
        public uint BaseInstance { get; init; }
    }
}
