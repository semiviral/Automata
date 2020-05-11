#region

using System.Numerics;
using Automata.Core;
using Automata.Core.Components;

#endregion

namespace Automata.Input
{
    public class KeyboardInputTranslation : IComponent
    {
        public Vector3 Value { get; set; }
    }
}
