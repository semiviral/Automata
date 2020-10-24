#region

using System;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class RenderShader : IComponent, IDisposable
    {
        private static readonly Shader _DefaultShader = new Shader();

        public Shader Value { get; set; } = _DefaultShader;

        public void Dispose() => Value.Dispose();
    }
}
