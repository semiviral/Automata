using System;
using System.Diagnostics.SymbolStore;
using Automata.Engine;
using Automata.Engine.Numerics;
using Automata.Game.Chunks.Generation;

namespace Automata.Game.Chunks
{
    public class ChunkLoader : Component
    {
        private int _Radius;
        private int _RadiusInBlocks;

        public bool RadiusChanged { get; set; }

        public int Radius
        {
            get => _Radius;
            set
            {
                _Radius = value;
                _RadiusInBlocks = value * GenerationConstants.CHUNK_SIZE;
                RadiusChanged = true;
            }
        }

        public int RadiusInBlocks => _RadiusInBlocks;

        public Vector3<int> Origin { get; set; } = new Vector3<int>(int.MaxValue);

        public bool IsWithinRadius(Vector3<int> origin)
        {
            Vector3<int> difference = (Origin - origin).WithY(0);

            return Vector.All(Vector3<int>.Abs(difference) <= RadiusInBlocks);
        }
    }
}
