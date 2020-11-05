using System.Runtime.InteropServices;

namespace Automata.Game
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PackedVertex
    {
        public readonly int LocalCoordinates;
        public readonly int UVCoordinates;

        public PackedVertex(int localCoordinates, int uvCoordinates) => (LocalCoordinates, UVCoordinates) = (localCoordinates, uvCoordinates);
    }
}
