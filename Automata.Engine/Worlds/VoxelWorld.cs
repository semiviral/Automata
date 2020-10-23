#region

using System.Collections.Generic;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;

#endregion


namespace Automata.Engine.Worlds
{
    public class VoxelWorld : GameWorld
    {
        private Dictionary<Vector3i, IEntity> _Chunks;

        public IReadOnlyCollection<IEntity> Chunks => _Chunks.Values;

        public VoxelWorld(bool active) : base(active) { }
    }
}
