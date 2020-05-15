#region

using System.Numerics;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Singletons;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Core
{
    public class MovementSystem : ComponentSystem
    {
        public MovementSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(InputListener),
                typeof(Translation),
                typeof(Rotation)
            };

            Input.Validate();
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            Vector3 movementVector = GetMovementVector(deltaTime);

            if (movementVector == Vector3.Zero)
            {
                return;
            }

            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<InputListener, Translation>())
            {
                entity.GetComponent<Translation>().Value += entity.TryGetComponent(out Rotation rotation)
                    ? Vector3.Transform(movementVector, rotation.Value)
                    : movementVector;
            }
        }

        private static Vector3 GetMovementVector(float deltaTime)
        {
            Vector3 movementVector = Vector3.Zero;

            if (Input.Instance.IsKeyPressed(Key.W))
            {
                movementVector += Vector3.UnitZ * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.S))
            {
                movementVector -= Vector3.UnitZ * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.D))
            {
                movementVector -= Vector3.UnitX * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.A))
            {
                movementVector += Vector3.UnitX * deltaTime;
            }

            return movementVector;
        }
    }
}
