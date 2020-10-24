using System;

namespace Automata.Engine.Rendering.Meshes
{
    public interface IMesh : IDisposable
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public uint IndexesLength { get; }

        public void BindVertexArrayObject();
    }
}
