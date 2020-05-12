using Automata.Collections;
using Automata.Core.Components;

namespace AutomataTest
{
    public class BlockData : IComponent
    {
        private INodeCollection<ushort> Blocks { get; set; }
    }
}
