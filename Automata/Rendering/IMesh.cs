namespace Automata.Rendering
{
    public interface IMesh
    {
        public uint IndexesCount { get; }

        public void BindVertexArrayObject();
    }
}
