namespace Automata.Rendering
{
    public interface IMesh
    {
        public uint IndexesLength { get; }

        public void BindVertexArrayObject();
    }
}
