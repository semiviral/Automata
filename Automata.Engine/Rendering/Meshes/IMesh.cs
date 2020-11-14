using System;

namespace Automata.Engine.Rendering.Meshes
{
    public interface IMesh : IDisposable
    {
        public Guid ID { get; }
        public Layer Layer { get; }
        public bool Visible { get; }

        public void Draw();
    }
}
