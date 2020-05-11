#region

using Automata.Core;
using Automata.Core.Components;
using Automata.Rendering.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderedShader : IComponent
    {
        public Shader Shader { get; set; }
    }
}
