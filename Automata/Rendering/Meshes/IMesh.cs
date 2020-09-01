using Silk.NET.OpenAL;

namespace Automata.Rendering.Meshes
{
    public interface IMesh
    {
        public bool Visible { get; }
        public uint IndexesLength { get; }

        public void BindVertexArrayObject();
    }
}
