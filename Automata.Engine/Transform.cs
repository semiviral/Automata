using System.Numerics;
using Automata.Engine.Numerics;

namespace Automata.Engine
{
    public class Transform : Component
    {
        private float _Scale;
        private Quaternion _Rotation;
        private Vector3<float> _Translation;

        public Matrix4x4 Matrix { get; set; }
        public bool Changed { get; set; }

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

        public Vector3<float> Translation
        {
            get => _Translation;
            set
            {
                _Translation = value;
                Changed = true;
            }
        }

        public Transform()
        {
            Scale = 1f;
            Rotation = Quaternion.Identity;
            Translation = Vector3<float>.Zero;
        }

        public override string ToString() =>
            $"{nameof(Transform)} {{ {nameof(Translation)} = {Translation}, {nameof(Rotation)} = {Rotation}, {nameof(Scale)} = {Scale} }}";
    }
}
