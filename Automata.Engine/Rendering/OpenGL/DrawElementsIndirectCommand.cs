using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DrawElementsIndirectCommand
    {
        public readonly uint VertexCount;
        public readonly uint InstanceCount;
        public readonly uint AbsoluteVertexOffset;
        public readonly uint RelativeVertexOffset;
        public readonly uint BaseInstance;

        public DrawElementsIndirectCommand(uint vertexCount, uint instanceCount, uint absoluteVertexOffset, uint relativeVertexOffset, uint baseInstance)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            AbsoluteVertexOffset = absoluteVertexOffset;
            RelativeVertexOffset = relativeVertexOffset;
            BaseInstance = baseInstance;
        }
    }
}
