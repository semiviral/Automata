#region

using Automata.Core.Components;
using Automata.Rendering.OpenGL;
using Automata.Singletons;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Mesh : IComponent
    {
        public BufferObject<float> VertexesBuffer { get; set; }
        public BufferObject<uint> IndexesBuffer { get; set; }
        public VertexArrayObject<float, uint> VertexArrayObject { get; set; }

        public Mesh()
        {
            VertexesBuffer = new BufferObject<float>(GLAPI.Instance.GL, BufferTargetARB.ArrayBuffer);
            IndexesBuffer = new BufferObject<uint>(GLAPI.Instance.GL, BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject = new VertexArrayObject<float, uint>(GLAPI.Instance.GL, VertexesBuffer, IndexesBuffer);
        }
    }
}
