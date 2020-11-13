using System.IO;
using Automata.Engine;
using Automata.Game.Extensions;
using Tomlyn;

namespace Automata.Game
{
    public class Settings : Singleton<Settings>
    {
        [TomlProperty("Video", false)]
        public bool VSync { get; init; } = false;

        [TomlProperty("WorldGeneration", true)]
        public bool SingleThreadedGeneration { get; init; }

        [TomlProperty("WorldGeneration", true)]
        public int GenerationRadius { get; init; }

        [TomlProperty("Diagnostic", false)]
        public bool DisableTemporalDiagnostics { get; init; } = false;

        [TomlProperty("Diagnostic", false)]
        public int DebugDataBufferSize { get; init; } = 120;


        public static void Load() => AssignInstance(Toml.Parse(File.ReadAllText("Settings.toml")).ToModel<Settings>());
    }
}
