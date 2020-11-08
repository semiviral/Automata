using Automata.Engine;

namespace Automata.Game
{
    public class Settings : Singleton<Settings>
    {
        public bool SingleThreadedGeneration { get; }
    }
}
