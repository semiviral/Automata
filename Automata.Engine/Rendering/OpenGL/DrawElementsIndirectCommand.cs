using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DrawElementsIndirectCommand
    {
        public uint VertexCount { get; init; }
        public uint InstanceCount { get; init; }
        public uint AbsoluteVertexOffset { get; init; }
        public uint RelativeVertexOffset { get; init; }
        public uint BaseInstance { get; init; }
    }
}
