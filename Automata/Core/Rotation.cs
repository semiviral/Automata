#region

using System.Numerics;

#endregion

namespace Automata.Core
{
    public class Rotation : IComponent
    {
        private Quaternion _Value = Quaternion.Identity;

        public Quaternion Value
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
