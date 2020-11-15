using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Quad<TVertex> : IEquatable<Quad<TVertex>> where TVertex : unmanaged
    {
        public readonly QuadIndexes Indexes;
        public readonly QuadVertexes<TVertex> Vertexes;

        public Quad(QuadIndexes indexes, QuadVertexes<TVertex> vertexes)
        {
            Indexes = indexes;
            Vertexes = vertexes;
        }

        public bool Equals(Quad<TVertex> other) => Indexes.Equals(other.Indexes) && Vertexes.Equals(other.Vertexes);
        public override bool Equals(object? obj) => obj is Quad<TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Indexes, Vertexes);

        public static bool operator ==(Quad<TVertex> left, Quad<TVertex> right) => left.Equals(right);
        public static bool operator !=(Quad<TVertex> left, Quad<TVertex> right) => !left.Equals(right);
    }
}
