#region

using System;
using Automata.Components;

#endregion

namespace AutomataTest.Chunks
{
    public class ChunkID : IComponent
    {
        public Guid Value { get; } = Guid.NewGuid();
    }
}
