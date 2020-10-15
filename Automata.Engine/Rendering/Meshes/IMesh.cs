namespace Automata.Engine.Rendering.Meshes
{
    public interface IMesh
    {
        public bool Visible { get; }
        public uint IndexesLength { get; }

        public void BindVertexArrayObject();
    }
}
