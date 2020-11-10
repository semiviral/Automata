using System;

namespace Automata.Game.Blocks
{
    public struct Block : IEquatable<Block>
    {
        public ushort ID { get; }
        public ushort Color { get; set; }
        public byte LightLevel { get; set; }

        public Block(ushort id)
        {
            ID = id;
            Color = 0;
            LightLevel = 0;
        }

        public bool Equals(Block other) =>
            (ID == other.ID)
            && (Color == other.Color)
            && (LightLevel == other.LightLevel);

        public override bool Equals(object? obj) => obj is Block other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ID, Color, LightLevel);

        public static bool operator ==(Block left, Block right) => left.Equals(right);
        public static bool operator !=(Block left, Block right) => !left.Equals(right);
    }
}
