using System;
using System.Runtime.InteropServices;

namespace Automata.Game.Chunks.Generation.Meshing
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PackedVertex : IEquatable<PackedVertex>
    {
        public readonly int LocalCoordinates;
        public readonly int UVCoordinates;

        public PackedVertex(int localCoordinates, int uvCoordinates) => (LocalCoordinates, UVCoordinates) = (localCoordinates, uvCoordinates);

        public bool Equals(PackedVertex other) => (LocalCoordinates == other.LocalCoordinates) && (UVCoordinates == other.UVCoordinates);
        public override bool Equals(object? obj) => obj is PackedVertex other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(LocalCoordinates, UVCoordinates);

        public static bool operator ==(PackedVertex left, PackedVertex right) => left.Equals(right);
        public static bool operator !=(PackedVertex left, PackedVertex right) => !left.Equals(right);
    }
}
