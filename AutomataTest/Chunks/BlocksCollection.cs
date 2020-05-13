#region

using Automata.Collections;
using Automata.Core.Components;
using AutomataTest.Chunks.Generation;

#endregion

namespace AutomataTest.Chunks
{
    public class BlocksCollection : IComponent
    {
        public INodeCollection<ushort> Blocks { get; set; } = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, 0, false);
    }
}
