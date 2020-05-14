#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Singletons;

#endregion

namespace Automata.Core
{
    public class RotationSystem : ComponentSystem
    {
        public RotationSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Rotation rotation in entityManager.GetComponents<Rotation>())
            {
                Vector2 offset = Vector2.Clamp(Input.Instance.GetMousePositionRelative(), new Vector2(-1f), Vector2.One);

                if (offset == Vector2.Zero)
                {
                    continue;
                }
                rotation.Value *=  Quaternion.CreateFromAxisAngle(new Vector3(offset.Y, offset.X, 0f), deltaTime);
            }

            Input.Instance.SetMousePositionRelative(0, Vector2.Zero);
        }
    }
}
