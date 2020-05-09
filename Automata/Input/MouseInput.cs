#region

using System.Numerics;
using Automata.Core;

#endregion

namespace Automata.Input
{
    public class MouseInput : IComponent
    {
        private Vector2 _Value;

        public Vector2 Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public bool Changed { get; set; }
    }
}
