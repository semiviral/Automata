using Automata.Engine.Numerics;

namespace Automata.Game.Chunks
{
    public record ChunkModification
    {
        public Vector3i Local { get; init; }
        public ushort BlockID { get; init; }
    }
}
