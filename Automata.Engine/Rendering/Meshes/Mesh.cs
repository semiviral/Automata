#region

using Automata.Engine.Rendering.OpenGL;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Engine.Rendering.Meshes
{
    public class Mesh<T> : IMesh where T : unmanaged
    {
        public bool Visible { get; }
        public BufferObject<T> VertexesBuffer { get; }
        public BufferObject<uint> IndexesBuffer { get; }
        public VertexArrayObject<T, uint> VertexArrayObject { get; }

        public uint IndexesLength => IndexesBuffer.Length;

        public Mesh()
        {
            Visible = true;
            VertexesBuffer = new BufferObject<T>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<T, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }

        public void BindVertexArrayObject()
        {
            VertexArrayObject.Bind();
        }
    }
}
