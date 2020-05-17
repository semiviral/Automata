#region

using Automata.Numerics;

#endregion

namespace Automata.Worlds
{
    public class Translation : IComponentChangeable
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
