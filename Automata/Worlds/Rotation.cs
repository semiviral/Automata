#region

using System.Numerics;

#endregion

namespace Automata.Worlds
{
    public class Rotation : IComponentChangeable
    {
        private Quaternion _Value;

        public bool Changed { get; set; }

        public Quaternion Value
        {
            get => _Value;
            set
            {
                _Value = value;
                Changed = true;
            }
        }

        public Rotation() => Value = Quaternion.Identity;
    }
}
