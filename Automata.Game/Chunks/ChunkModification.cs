using Automata.Engine.Numerics;

namespace Automata.Game.Chunks
{
    public sealed record ChunkModification
    {
        public int BlockIndex { get; init; }
        public ushort BlockID { get; init; }
    }
}
