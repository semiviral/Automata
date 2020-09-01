#region

using System;
using System.Numerics;
using Automata.Entity;
using Automata.System;

#endregion

namespace AutomataTest
{
    public class RotationTestSystem : ComponentSystem
    {
        private float _AccumulatedTime;

        public RotationTestSystem()
        {
            HandledComponentTypes = new ComponentTypes(typeof(Rotation), typeof(RotationTest));

            Enabled = false;
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            Quaternion newRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _AccumulatedTime);

            foreach ((_, Rotation rotation) in entityManager.GetComponents<RotationTest, Rotation>())
            {
                rotation.Value = newRotation;
            }

            _AccumulatedTime += (float)delta.TotalSeconds;
        }
    }
}
