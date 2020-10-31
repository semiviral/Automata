#region

using System;
using Automata.Engine.Components;
using Automata.Engine.Rendering.OpenGL.Textures;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class Material : IComponent, IDisposable
    {
        public Shader Shader { get; set; }
        public Texture?[] Textures { get; }

        public Material(Shader value) => (Shader, Textures) = (value, new Texture?[9] /* 9 OGL texture channels */);

        public void Dispose() => Shader.Dispose();
    }
}
