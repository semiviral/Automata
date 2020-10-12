#region

using System.Numerics;

#endregion

namespace Automata.Components
{
    public class Translation : IComponentChangeable
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

        public bool Changed { get; set; }
    }
}
