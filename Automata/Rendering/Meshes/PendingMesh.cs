namespace Automata.Rendering.Meshes
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public class PendingMesh<TDataType> where TDataType : unmanaged
    {
        public TDataType[] Vertexes { get; }
        public uint[] Indexes { get; }

        public PendingMesh(TDataType[] vertexes, uint[] indexes)
        {
            Vertexes = vertexes;
            Indexes = indexes;
        }
    }
}
