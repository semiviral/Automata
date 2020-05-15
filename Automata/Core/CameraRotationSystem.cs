#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Rendering;
using Automata.Singletons;

#endregion

namespace Automata.Core
{
    public class CameraRotationSystem : ComponentSystem
    {
        public CameraRotationSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            Vector2 offset = Vector2.Clamp(Input.Instance.GetMousePositionRelative(), new Vector2(-1f), Vector2.One);

            if (offset == Vector2.Zero)
            {
                return;
            }

            foreach ((Camera _, Rotation rotation) in entityManager.GetComponents<Camera, Rotation>())
            {
                rotation.Value *= Quaternion.CreateFromAxisAngle(new Vector3(offset.Y, offset.X, 0f), deltaTime * 10f);
            }

            Input.Instance.SetMousePositionRelative(0, Vector2.Zero);
        }
    }
}
