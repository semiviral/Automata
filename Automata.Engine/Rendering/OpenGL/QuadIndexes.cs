using System;
using System.Runtime.InteropServices;

namespace Automata.Engine.Rendering.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QuadIndexes<TIndex> : IEquatable<QuadIndexes<TIndex>> where TIndex : unmanaged, IEquatable<TIndex>
    {
        public readonly TIndex Index1;
        public readonly TIndex Index2;
        public readonly TIndex Index3;
        public readonly TIndex Index4;
        public readonly TIndex Index5;
        public readonly TIndex Index6;

        public QuadIndexes(TIndex index1, TIndex index2, TIndex index3, TIndex index4, TIndex index5, TIndex index6)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
            Index4 = index4;
            Index5 = index5;
            Index6 = index6;
        }

        public bool Equals(QuadIndexes<TIndex> other) =>
            Index1.Equals(other.Index1)
            && Index2.Equals(other.Index2)
            && Index3.Equals(other.Index3)
            && Index4.Equals(other.Index4)
            && Index5.Equals(other.Index5)
            && Index6.Equals(other.Index6);

        public override bool Equals(object? obj) => obj is QuadIndexes<TIndex> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Index1, Index2, Index3, Index4, Index5, Index6);

        public static bool operator ==(QuadIndexes<TIndex> left, QuadIndexes<TIndex> right) => left.Equals(right);
        public static bool operator !=(QuadIndexes<TIndex> left, QuadIndexes<TIndex> right) => !left.Equals(right);
    }
}
