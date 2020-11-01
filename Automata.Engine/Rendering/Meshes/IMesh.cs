using System;

namespace Automata.Engine.Rendering.Meshes
{
    public interface IMesh : IDisposable
    {
        public Guid ID { get; }
        public bool Visible { get; }
        public Layer Layer { get; }

        public uint IndexesLength { get; }
        public uint IndexesByteLength { get; }

        public void Bind();
        public void Unbind();
    }
}
