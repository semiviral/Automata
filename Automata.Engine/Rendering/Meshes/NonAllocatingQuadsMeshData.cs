using System;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public record NonAllocatingQuadsMeshData<TVertex> : IDisposable where TVertex : unmanaged, IEquatable<TVertex>
    {
        public static readonly NonAllocatingQuadsMeshData<TVertex> Empty =
            new NonAllocatingQuadsMeshData<TVertex>(NonAllocatingList<QuadVertexes<TVertex>>.Empty, NonAllocatingList<QuadIndexes>.Empty);

        public NonAllocatingList<QuadVertexes<TVertex>> Vertexes { get; }
        public NonAllocatingList<QuadIndexes> Indexes { get; }

        public bool IsEmpty => Vertexes.IsEmpty && Indexes.IsEmpty;

        public NonAllocatingQuadsMeshData(NonAllocatingList<QuadVertexes<TVertex>> vertexes, NonAllocatingList<QuadIndexes> indexes) =>
            (Vertexes, Indexes) = (vertexes, indexes);

        public void Dispose()
        {
            Vertexes.Dispose();
            Indexes.Dispose();
        }
    }
}
