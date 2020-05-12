using Automata.Collections;
using Automata.Core.Components;

namespace AutomataTest.Chunks
{
    public class ChunkData : IComponent
    {
        private INodeCollection<ushort> Blocks { get; set; }
    }
}
