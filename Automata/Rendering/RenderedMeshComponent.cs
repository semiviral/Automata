#region

using Automata.Core;
using Automata.Core.Components;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderedMeshComponent : IComponent
    {
        public VertexBuffer? VertexBuffer { get; set; }
        public BufferObject<uint>? BufferObject { get; set; }
        public VertexArrayObject<float, uint>? VertexArrayObject { get; set; }
    }
}
