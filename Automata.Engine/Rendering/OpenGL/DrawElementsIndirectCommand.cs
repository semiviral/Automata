using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DrawElementsIndirectCommand
    {
        public readonly uint Count;
        public readonly uint InstanceCount;
        public readonly uint FirstIndexOffset;
        public readonly uint BaseVertex;
        public readonly uint BaseInstance;
    }
}
