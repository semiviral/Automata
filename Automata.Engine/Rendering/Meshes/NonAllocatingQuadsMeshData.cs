using System;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public record NonAllocatingQuadsMeshData<TIndex, TVertex> : IDisposable
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        public static readonly NonAllocatingQuadsMeshData<TIndex, TVertex> Empty =
            new NonAllocatingQuadsMeshData<TIndex, TVertex>(NonAllocatingList<QuadIndexes<TIndex>>.Empty, NonAllocatingList<QuadVertexes<TVertex>>.Empty);

        public NonAllocatingList<QuadIndexes<TIndex>> Indexes { get; }
        public NonAllocatingList<QuadVertexes<TVertex>> Vertexes { get; }

        public bool IsEmpty => Indexes.IsEmpty && Vertexes.IsEmpty;

        public NonAllocatingQuadsMeshData(NonAllocatingList<QuadIndexes<TIndex>> indexes, NonAllocatingList<QuadVertexes<TVertex>> vertexes)
        {
            Indexes = indexes;
            Vertexes = vertexes;
        }

        public void Dispose()
        {
            Indexes.Dispose();
            Vertexes.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
