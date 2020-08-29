#region

using System.Numerics;

#endregion

namespace Automata.Entity
{
    public class Rotation : IComponentChangeable
    {
        private Quaternion _Value;

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

        public bool Changed { get; set; }
    }
}
