using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quad<TIndex, TVertex> : IEquatable<Quad<TIndex, TVertex>>
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        public readonly QuadIndexes<TIndex> Indexes;
        public readonly QuadVertexes<TVertex> Vertexes;

        public Quad(QuadIndexes<TIndex> indexes, QuadVertexes<TVertex> vertexes)
        {
            Indexes = indexes;
            Vertexes = vertexes;
        }

        public bool Equals(Quad<TIndex, TVertex> other) => Indexes.Equals(other.Indexes) && Vertexes.Equals(other.Vertexes);
        public override bool Equals(object? obj) => obj is Quad<TIndex, TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Indexes, Vertexes);

        public static bool operator ==(Quad<TIndex, TVertex> left, Quad<TIndex, TVertex> right) => left.Equals(right);
        public static bool operator !=(Quad<TIndex, TVertex> left, Quad<TIndex, TVertex> right) => !left.Equals(right);
    }
}
