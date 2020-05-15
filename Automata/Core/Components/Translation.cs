#region

using Automata.Numerics;

#endregion

namespace Automata.Core.Components
{
    public class Translation : IComponent
    {
        private Vector3d _Value;

        public bool Changed { get; set; }

        public Vector3d Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public Translation() => Value = Vector3d.Zero;
    }
}
