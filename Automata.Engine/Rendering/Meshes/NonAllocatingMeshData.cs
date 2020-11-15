using System;
using Automata.Engine.Collections;

namespace Automata.Engine.Rendering.Meshes
{
    public record NonAllocatingMeshData<TVertex> : IDisposable where TVertex : unmanaged, IEquatable<TVertex>
    {
        public static readonly NonAllocatingMeshData<TVertex> Empty =
            new NonAllocatingMeshData<TVertex>(NonAllocatingList<TVertex>.Empty, NonAllocatingList<uint>.Empty);

        public NonAllocatingList<TVertex> Vertexes { get; }
        public NonAllocatingList<uint> Indexes { get; }

        public bool IsEmpty => Vertexes.IsEmpty && Indexes.IsEmpty;

        public NonAllocatingMeshData(NonAllocatingList<TVertex> vertexes, NonAllocatingList<uint> indexes) => (Vertexes, Indexes) = (vertexes, indexes);

        public void Dispose()
        {
            Vertexes.Dispose();
            Indexes.Dispose();
        }
    }
}
