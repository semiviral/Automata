using System.Collections.Generic;
using Automata.Engine;

namespace Automata.Game.Biomes
{
    public class BiomeRegistry : Singleton<BiomeRegistry>
    {
        private readonly List<Biome> _Biomes;
        private readonly Dictionary<string, int> _BiomesIndexer;

        public BiomeRegistry()
        {
            _Biomes = new List<Biome>();
            _BiomesIndexer = new Dictionary<string, int>();
        }
    }
}
