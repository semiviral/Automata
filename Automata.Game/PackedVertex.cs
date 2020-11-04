using System.Runtime.InteropServices;

namespace Automata.Game
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PackedVertex
    {
        public int LocalCoordinates { get; }
        public int UVCoordinates { get; }

        public PackedVertex(int localCoordinates, int uvCoordinates) => (LocalCoordinates, UVCoordinates) = (localCoordinates, uvCoordinates);
    }
}
