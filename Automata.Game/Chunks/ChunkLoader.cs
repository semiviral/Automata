using Automata.Engine;
using Automata.Engine.Numerics;

namespace Automata.Game.Chunks
{
    public class ChunkLoader : Component
    {
        private int _Radius;

        public bool RadiusChanged { get; set; }

        public int Radius
        {
            get => _Radius;
            set
            {
                _Radius = value;
                RadiusChanged = true;
            }
        }

        public Vector3i Origin { get; set; } = new Vector3i(int.MaxValue);
    }
}
