#region

using Automata.Core;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderedShaderComponent : IComponent
    {
        public Shader Shader { get; set; }
    }
}
