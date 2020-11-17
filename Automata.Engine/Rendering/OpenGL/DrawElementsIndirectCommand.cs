using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DrawElementsIndirectCommand
    {
        public readonly uint VertexCount;
        public readonly uint InstanceCount;

        /// <summary>
        ///     Offset of your first index.
        /// </summary>
        /// <remarks>
        ///     <p>
        ///         This offset is measured in units of whatever the size of your index type is.
        ///     </p>
        ///     <p>
        ///         So, for an an index type of uint, that means units of 4 bytes. So, if allocating from a shared buffer, ensure alignment of the rented slices.
        ///     </p>
        /// </remarks>
        public readonly uint FirstIndexOffset;

        public readonly uint FirstVertexOffset;
        public readonly uint BaseInstance;

        public DrawElementsIndirectCommand(uint vertexCount, uint instanceCount, uint firstIndexOffset, uint firstVertexOffset, uint baseInstance)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            FirstIndexOffset = firstIndexOffset;
            FirstVertexOffset = firstVertexOffset;
            BaseInstance = baseInstance;
        }
    }
}
