using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QuadVertexes<TVertex> : IEquatable<QuadVertexes<TVertex>> where TVertex : unmanaged
    {
        public readonly TVertex Vertex1;
        public readonly TVertex Vertex2;
        public readonly TVertex Vertex3;
        public readonly TVertex Vertex4;

        public QuadVertexes(TVertex vertex1, TVertex vertex2, TVertex vertex3, TVertex vertex4)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
            Vertex4 = vertex4;
        }

        public bool Equals(QuadVertexes<TVertex> other) =>
            Vertex1.Equals(other.Vertex1)
            && Vertex2.Equals(other.Vertex2)
            && Vertex3.Equals(other.Vertex3)
            && Vertex4.Equals(other.Vertex4);

        public override bool Equals(object? obj) => obj is QuadVertexes<TVertex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Vertex1, Vertex2, Vertex3, Vertex4);

        public static bool operator ==(QuadVertexes<TVertex> left, QuadVertexes<TVertex> right) => left.Equals(right);
        public static bool operator !=(QuadVertexes<TVertex> left, QuadVertexes<TVertex> right) => !left.Equals(right);
    }
}
