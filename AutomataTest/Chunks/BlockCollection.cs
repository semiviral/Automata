using Automata.Collections;
using Automata.Core.Components;

namespace AutomataTest.Chunks
{
    public class BlockCollection : IComponent
    {
        public INodeCollection<ushort> Blocks { get; set; }
    }
}
