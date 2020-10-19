#region

using System.Numerics;

#endregion

namespace Automata.Engine.Components
{
    public class Rotation : IComponentChangeable
    {
        private Vector3 _AccumulatedAngles = Vector3.Zero;
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

        public Rotation() => Value = Quaternion.Identity;

        public bool Changed { get; set; }

        public void AccumulateAngles(Vector3 axisAngles)
        {
            _AccumulatedAngles += axisAngles;

            // create quaternions based on local angles
            Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, _AccumulatedAngles.X);
            Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _AccumulatedAngles.Y);

            // rotate around (pitch as global) and (yaw as local)
            Value = pitch * yaw;
        }
    }
}
