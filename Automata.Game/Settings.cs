using System.IO;
using Automata.Engine;
using Tomlyn;

namespace Automata.Game
{
    public class Settings : Singleton<Settings>
    {
        [TomlProperty("Video", false)]
        public bool VSync { get; set; } = false;

        [TomlProperty("WorldGeneration", true)]
        public bool SingleThreadedGeneration { get; set; }

        [TomlProperty("WorldGeneration", true)]
        public int GenerationRadius { get; set; }

        public static void Load() => AssignInstance(Toml.Parse(File.ReadAllText("Settings.toml")).ToModel<Settings>());
    }
}
