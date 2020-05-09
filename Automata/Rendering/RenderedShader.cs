#region

using Automata.Core;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderedShader : IComponent
    {
        public Shader Shader { get; set; }
    }
}
