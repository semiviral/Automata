using System;

namespace Automata.Engine.Rendering.Meshes
{
    public interface IMesh : IDisposable
    {
        public bool Visible { get; }
        public uint IndexesLength { get; }

        public void BindVertexArrayObject();
    }
}
