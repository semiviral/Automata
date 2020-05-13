#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Singletons;

#endregion

namespace Automata.Core.Systems
{
    public class RotationSystem : ComponentSystem
    {
        private Vector2 _LastFrameMouseOffset;

        public RotationSystem()
        {
            _LastFrameMouseOffset = Vector2.Zero;

            HandledComponentTypes = new[]
            {
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Rotation rotation in entityManager.GetComponents<Rotation>())
            {
                Vector2 offset = GameWindow.Instance.Size - Input.Instance.GetMousePosition(0);

                if (offset == _LastFrameMouseOffset)
                {
                    continue;
                }

                _LastFrameMouseOffset = offset;

                Quaternion axisAngleQuaternion = Quaternion.CreateFromAxisAngle(new Vector3(offset, 0f), deltaTime);
                Quaternion finalRotationPosition = Quaternion.Add(rotation.Value, axisAngleQuaternion);

                rotation.Value = Quaternion.Lerp(rotation.Value, finalRotationPosition, deltaTime);
            }
        }
    }
}
