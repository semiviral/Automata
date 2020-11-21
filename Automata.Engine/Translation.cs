using System.Numerics;

namespace Automata.Engine.Components
{
    public class Translation : ComponentChangeable
    {
        private Vector3 _Value;

        public Vector3 Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public Translation() => Value = Vector3.Zero;
    }
}
