using System.Threading;
using Automata.Core.Components;

namespace AutomataTest
{
    public class Cancellable : IComponent
    {
        public CancellationTokenSource TokenSource { get; set; }
    }
}
