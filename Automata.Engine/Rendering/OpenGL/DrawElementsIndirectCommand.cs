using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DrawElementsIndirectCommand
    {
        public readonly uint VertexCount;
        public readonly uint InstanceCount;
        public readonly uint ObjectVertexOffset;
        public readonly uint BaseVertexOffset;
        public readonly uint BaseInstance;

        public DrawElementsIndirectCommand(uint vertexCount, uint instanceCount, uint objectVertexOffset, uint baseVertexOffset, uint baseInstance)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            ObjectVertexOffset = objectVertexOffset;
            BaseVertexOffset = baseVertexOffset;
            BaseInstance = baseInstance;
        }
    }
}
