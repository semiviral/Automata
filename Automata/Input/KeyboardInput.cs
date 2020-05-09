#region

using System.Collections.Generic;
using Automata.Core;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    public class KeyboardInput : IComponent
    {
        public HashSet<Key> KeysUp { get; } = new HashSet<Key>();
        public HashSet<Key> KeysDown { get; } = new HashSet<Key>();
    }
}
