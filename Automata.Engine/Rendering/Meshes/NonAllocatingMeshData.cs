using System;
using Automata.Engine.Collections;

namespace Automata.Engine.Rendering.Meshes
{
    public record NonAllocatingMeshData<TVertex> : IDisposable where TVertex : unmanaged, IEquatable<TVertex>
    {
        public static readonly NonAllocatingMeshData<TVertex> Empty = new NonAllocatingMeshData<TVertex>(MemoryList<TVertex>.Empty, MemoryList<uint>.Empty);

        public MemoryList<TVertex> Vertexes { get; }
        public MemoryList<uint> Indexes { get; }

        public bool IsEmpty => Vertexes.IsEmpty && Indexes.IsEmpty;

        public NonAllocatingMeshData(MemoryList<TVertex> vertexes, MemoryList<uint> indexes) => (Vertexes, Indexes) = (vertexes, indexes);

        public void Dispose()
        {
            Vertexes.Dispose();
            Indexes.Dispose();
        }
    }
}
