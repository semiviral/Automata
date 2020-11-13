using System;
using Silk.NET.Input.Common;

namespace Automata.Engine.Input
{
    public class InputAction
    {
        public Key[] KeyCombination { get; }
        public Action Action { get; }
        public bool Active { get; set; }

        public InputAction(Action action, params Key[] keyCombination) => (Action, KeyCombination) = (action, keyCombination);
    }
}
