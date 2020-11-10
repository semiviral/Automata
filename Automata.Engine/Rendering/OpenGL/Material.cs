#region

using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class Material : Component
    {
        public ProgramPipeline Pipeline { get; set; }
        public Texture?[] Textures { get; }

        public Material(ProgramPipeline pipeline) => (Pipeline, Textures) = (pipeline, new Texture?[9] /* 9 OGL texture channels */);
    }
}
