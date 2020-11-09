using System;

namespace Automata.Game.Biomes
{
    public class Biome : IEquatable<Biome>
    {
        public Guid Identifier { get; }
        public string Name { get; }

        public Biome(string name)
        {

        }

        public bool Equals(Biome? other) =>
            other is not null && Identifier.Equals(other.Identifier) && Name.Equals(other.Name, StringComparison.InvariantCulture);

        public override bool Equals(object? obj) => obj is Biome biome && Equals(biome);

        public override int GetHashCode() => HashCode.Combine(Identifier, Name);

        public static bool operator ==(Biome? left, Biome? right) => Equals(left, right);
        public static bool operator !=(Biome? left, Biome? right) => !Equals(left, right);
    }
}
