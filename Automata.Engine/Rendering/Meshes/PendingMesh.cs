using System;
using System.Collections.Generic;

namespace Automata.Engine.Rendering.Meshes
{
    /// <summary>
    ///     Used to hold pending mesh data that needs to be uploaded to the GPU.
    /// </summary>
    public sealed record PendingMesh<TDataType> where TDataType : unmanaged
    {
        public static PendingMesh<TDataType> Empty { get; } = new PendingMesh<TDataType>(Array.Empty<TDataType>(), Array.Empty<uint>());

        public Memory<TDataType> Vertexes { get; }
        public Memory<uint> Indexes { get; }

        public bool IsEmpty => (Vertexes.Length == 0) && (Indexes.Length == 0);

        public PendingMesh(Memory<TDataType> vertexes, Memory<uint> indexes) => (Vertexes, Indexes) = (vertexes, indexes);
    }
}
