#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Numerics;

#endregion

namespace Automata.Core.Systems
{
    public class CameraRotationSystem : ComponentSystem
    {
        private Vector2i _LastFrameMouseOffset;

        public CameraRotationSystem()
        {
            _LastFrameMouseOffset = Vector2i.Zero;

            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((Camera _, Rotation rotation) in entityManager.GetComponents<Camera, Rotation>())
            {
                Vector2i offset = InputSingleton.Instance.GetMousePosition(0);

                if (offset == _LastFrameMouseOffset) { }

                Vector3 mouseInputValue3d = new Vector3(mouseInput.Normal, 0f);
                Quaternion axisAngleQuaternion = Quaternion.CreateFromAxisAngle(mouseInputValue3d, 10f);
                Quaternion finalRotationPosition = Quaternion.Add(rotation.Value, axisAngleQuaternion);

                rotation.Value = Quaternion.Slerp(rotation.Value, finalRotationPosition, deltaTime);
            }
        }
    }
}
