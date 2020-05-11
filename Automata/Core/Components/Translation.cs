#region

using System.Numerics;

#endregion

namespace Automata.Core.Components
{
    public class Translation : IComponent
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

        public bool Changed { get; set; } = true;
    }
}
