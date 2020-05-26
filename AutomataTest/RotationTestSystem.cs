#region

using System;
using System.Numerics;
using Automata;
using Automata.Worlds;

#endregion

namespace AutomataTest
{
    public class RotationTestSystem : ComponentSystem
    {
        private float _AccumulatedTime;

        public RotationTestSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation),
                typeof(RotationTest)
            };
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
