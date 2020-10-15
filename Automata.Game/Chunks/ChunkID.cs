#region

using System;
using Automata.Engine.Components;

#endregion

namespace Automata.Game.Chunks
{
    public class ChunkID : IComponent
    {
        public Guid Value { get; } = Guid.NewGuid();
    }
}
