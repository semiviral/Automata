#region

using Automata.Engine.Components;
using Automata.Engine.Numerics;

#endregion


namespace Automata.Game.Chunks
{
    public class ChunkLoader : IComponentChangeable
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
        public bool Changed { get; set; }
    }
}
