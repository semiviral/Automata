#region

using System;
using Automata.Engine.Components;

#endregion


namespace Automata.Engine.Rendering.OpenGL
{
    public class RenderShader : IComponent, IDisposable
    {
        public Shader Value { get; set; }

        public RenderShader(Shader value) => Value = value;

        public void Dispose() => Value.Dispose();
    }
}
