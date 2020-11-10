#region

using System;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class Material : Component, IDisposable
    {
        public ProgramPipeline Pipeline { get; set; }
        public Texture?[] Textures { get; }

        public Material(ProgramPipeline pipeline) => (Pipeline, Textures) = (pipeline, new Texture?[9] /* 9 OGL texture channels */);

        public void Dispose() => Pipeline.Dispose();
    }
}
