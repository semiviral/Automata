#region

using System;
using System.Numerics;
using Automata.Core.Components;
using Automata.Core.Systems;
using Automata.Numerics;
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
            Vector3d movementVector = GetMovementVector(delta.TotalSeconds);

            if (Vector3b.All(movementVector == Vector3d.Zero))
            {
                return;
            }

            foreach (IEntity entity in entityManager.GetEntitiesWithComponents<InputListener, Translation>())
            {
                Vector3d transformedMovementVector = entity.TryGetComponent(out Rotation rotation)
                    ? Vector3d.Transform(movementVector, Quaternion.Conjugate(rotation.Value))
                    : movementVector;

                entity.GetComponent<Translation>().Value += transformedMovementVector;
            }
        }

        private static Vector3d GetMovementVector(double deltaTime)
        {
            Vector3d movementVector = Vector3d.Zero;

            if (Input.Instance.IsKeyPressed(Key.W))
            {
                movementVector += Vector3d.UnitZ * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.S))
            {
                movementVector -= Vector3d.UnitZ * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.D))
            {
                movementVector -= Vector3d.UnitX * deltaTime;
            }

            if (Input.Instance.IsKeyPressed(Key.A))
            {
                movementVector += Vector3d.UnitX * deltaTime;
            }

            return movementVector;
        }
    }
}
