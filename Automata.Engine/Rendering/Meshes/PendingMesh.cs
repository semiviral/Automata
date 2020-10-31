using System;

namespace Automata.Engine.Rendering.Meshes
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public class PendingMesh<TDataType> where TDataType : unmanaged
    {
        public static PendingMesh<TDataType> Empty { get; } = new PendingMesh<TDataType>(Array.Empty<TDataType>(), Array.Empty<uint>());

        public TDataType[] Vertexes { get; }
        public uint[] Indexes { get; }

        public bool IsEmpty => (Vertexes.Length == 0) && (Indexes.Length == 0);

        public PendingMesh(TDataType[] vertexes, uint[] indexes) => (Vertexes, Indexes) = (vertexes, indexes);
    }
}
