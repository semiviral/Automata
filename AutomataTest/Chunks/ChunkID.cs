#region

using System;
using Automata;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkID : IComponent
    {
        public Guid Value { get; } = Guid.NewGuid();
    }
}
