using Automata.Engine.Components;
using Automata.Engine.Numerics;

namespace Automata.Game.Chunks
{
    public class ChunkLoader : ComponentChangeable
    {
        private int _Radius;

        public int Radius
        {
            get => _Radius;
            set
            {
                _Radius = value;
                Changed = true;
            }
        }

        public Vector3i Origin { get; set; } = new Vector3i(int.MaxValue);
    }
}
