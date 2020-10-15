#region

using Automata.Engine.Collections;
using Automata.Engine.Components;
using Automata.Game.Chunks.Generation;

#endregion

namespace Automata.Game.Chunks
{
    public class BlocksCollection : IComponent
    {
        public INodeCollection<ushort> Value { get; set; } = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, 0, false);
    }
}
