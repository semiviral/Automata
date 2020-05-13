#region

using Automata.Core.Components;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class Mesh : IComponent
    {
        public BufferObject<float>? VertexBuffer { get; set; }
        public BufferObject<uint>? IndicesBuffer { get; set; }
        public VertexArrayObject<float, uint>? VertexArrayObject { get; set; }
    }
}
