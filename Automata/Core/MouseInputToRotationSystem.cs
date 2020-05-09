#region

using System.Numerics;
using Automata.Input;

#endregion

namespace Automata.Core
{
    public class MouseInputToRotationSystem : ComponentSystem
    {
        public MouseInputToRotationSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(MouseInput),
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((MouseInput mouseInput, Rotation rotation) in entityManager.GetComponents<MouseInput, Rotation>())
            {
                if (!mouseInput.Changed)
                {
                    continue;
                }

                Vector3 mouseInputValue3d = new Vector3(mouseInput.Normal, 0f);
                Quaternion axisAngleQuaternion = Quaternion.CreateFromAxisAngle(mouseInputValue3d, 10f);
                Quaternion finalRotationPosition = Quaternion.Add(rotation.Value, axisAngleQuaternion);

                rotation.Value = Quaternion.Slerp(rotation.Value, finalRotationPosition, deltaTime);
            }
        }
    }
}
