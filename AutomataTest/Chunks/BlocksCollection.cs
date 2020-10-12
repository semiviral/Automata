#region

using Automata.Collections;
using Automata.Components;
using AutomataTest.Chunks.Generation;

#endregion

namespace AutomataTest.Chunks
{
    public class BlocksCollection : IComponent
    {
        public INodeCollection<ushort> Value { get; set; } = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, 0, false);
    }
}
