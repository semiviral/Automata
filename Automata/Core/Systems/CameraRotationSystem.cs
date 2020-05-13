#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Rendering;

#endregion

namespace Automata.Core.Systems
{
    public class CameraRotationSystem : ComponentSystem
    {
        private Vector2 _LastFrameMouseOffset;

        public CameraRotationSystem()
        {
            _LastFrameMouseOffset = Vector2.Zero;

            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Camera camera, Rotation rotation) in entityManager.GetComponents<Camera, Rotation>())
            {
                Vector2 offset = Input.Instance.ViewCenter - Input.Instance.GetMousePosition(0);

                if (offset == _LastFrameMouseOffset)
                {
                    continue;
                }

                _LastFrameMouseOffset = offset;

                Quaternion axisAngleQuaternion = Quaternion.CreateFromAxisAngle(new Vector3(offset, 0f), deltaTime);
                Quaternion finalRotationPosition = Quaternion.Add(rotation.Value, axisAngleQuaternion);

                rotation.Value = Quaternion.Slerp(rotation.Value, finalRotationPosition, deltaTime);
                // update view
                camera.View = Matrix4x4.CreateFromQuaternion(rotation.Value);
            }
        }
    }
}
