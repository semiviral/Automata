#region

using Automata.Core;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    public class UnregisteredInputContext : IComponent
    {
        public IInputContext InputContext { get; set; }
    }
}
