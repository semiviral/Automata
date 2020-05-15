#region

using System;
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

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            Vector3 movementVector = GetMovementVector((float)delta.TotalSeconds);

            if (movementVector == Vector3.Zero)
            {
                return;
            }

            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<InputListener, Translation>())
            {
                Vector3 transformedMovementVector = entity.TryGetComponent(out Rotation rotation)
                    ? Vector3.Transform(movementVector, Quaternion.Conjugate(rotation.Value))
                    : movementVector;

                entity.GetComponent<Translation>().Value += transformedMovementVector;
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
