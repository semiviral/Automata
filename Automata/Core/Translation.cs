#region

using System.Numerics;

#endregion

namespace Automata.Core
{
    public class Translation : IComponent
    {
        private Vector3 _Position;

        public Vector3 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                Changed = true;
            }
        }

        public bool Changed { get; set; } = true;
    }
}
