#region

using System.Numerics;

#endregion

namespace Automata.Core.Components
{
    public class Rotation : IComponent
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
