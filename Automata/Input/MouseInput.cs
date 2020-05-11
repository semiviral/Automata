#region

using System.Numerics;
using Automata.Core;
using Automata.Core.Components;

#endregion

namespace Automata.Input
{
    public class MouseInput : IComponent
    {
        private Vector2 _Absolute;

        public Vector2 Absolute
        {
            get => _Absolute;
            set
            {
                _Absolute = value;
                Normal = Vector2.Clamp(_Absolute, new Vector2(1f), Vector2.One);
                Changed = true;
            }
        }

        public Vector2 Normal { get; private set; }
        public bool Changed { get; set; }
    }
}
