using System.Numerics;

namespace Automata.Engine
{
    public class Transform : ComponentChangeable
    {
        private float _Scale;
        private Quaternion _Rotation;
        private Vector3 _Translation;

        public Matrix4x4 Matrix { get; set; }

        public float Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Changed = true;
            }
        }

        public Quaternion Rotation
        {
            get => _Rotation;
            set
            {
                _Rotation = value;
                Changed = true;
            }
        }

        public Vector3 Translation
        {
            get => _Translation;
            set
            {
                _Translation = value;
                Changed = true;
            }
        }
    }
}
