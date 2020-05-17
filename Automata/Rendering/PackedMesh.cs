#region

using Automata.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class PackedMesh : IComponent
    {
        public BufferObject<int> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<int, uint> VertexArrayObject { get; }

        public PackedMesh()
        {
            VertexesBuffer = new BufferObject<int>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<int, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }
    }
}
