using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct VertexIndexes : IEquatable<VertexIndexes>
    {
        public readonly uint Index1;
        public readonly uint Index2;
        public readonly uint Index3;
        public readonly uint Index4;
        public readonly uint Index5;
        public readonly uint Index6;

        public VertexIndexes(uint index1, uint index2, uint index3, uint index4, uint index5, uint index6)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
            Index4 = index4;
            Index5 = index5;
            Index6 = index6;
        }

        public bool Equals(VertexIndexes other) =>
            (Index1 == other.Index1)
            && (Index2 == other.Index2)
            && (Index3 == other.Index3)
            && (Index4 == other.Index4)
            && (Index5 == other.Index5)
            && (Index6 == other.Index6);

        public override bool Equals(object? obj) => obj is VertexIndexes other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Index1, Index2, Index3, Index4, Index5, Index6);

        public static bool operator ==(VertexIndexes left, VertexIndexes right) => left.Equals(right);
        public static bool operator !=(VertexIndexes left, VertexIndexes right) => !left.Equals(right);
    }
}
