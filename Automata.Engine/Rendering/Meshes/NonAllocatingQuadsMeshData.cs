using System;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public record NonAllocatingQuadsMeshData<TVertex> : IDisposable where TVertex : unmanaged, IEquatable<TVertex>
    {
        public static readonly NonAllocatingQuadsMeshData<TVertex> Empty =
            new NonAllocatingQuadsMeshData<TVertex>(NonAllocatingList<QuadIndexes>.Empty, NonAllocatingList<QuadVertexes<TVertex>>.Empty);

        public NonAllocatingList<QuadIndexes> Indexes { get; }
        public NonAllocatingList<QuadVertexes<TVertex>> Vertexes { get; }

        public bool IsEmpty => Indexes.IsEmpty && Vertexes.IsEmpty;

        public NonAllocatingQuadsMeshData(NonAllocatingList<QuadIndexes> indexes, NonAllocatingList<QuadVertexes<TVertex>> vertexes)
        {
            Indexes = indexes;
            Vertexes = vertexes;
        }

        public void Dispose()
        {
            Indexes.Dispose();
            Vertexes.Dispose();
        }
    }
}
