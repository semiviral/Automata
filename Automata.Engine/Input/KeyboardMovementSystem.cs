#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Systems;
using Silk.NET.Input.Common;

#endregion


namespace Automata.Engine.Input
{
    public class KeyboardMovementSystem : ComponentSystem
    {
        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(KeyboardListener))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!AutomataWindow.Instance.Focused) return;

            Vector3 movementVector = -GetMovementVector((float)delta.TotalSeconds);

            if (movementVector == Vector3.Zero) return;

            foreach ((IEntity entity, Translation translation, KeyboardListener listener) in entityManager.GetEntities<Translation, KeyboardListener>())
            {
                movementVector = entity.TryGetComponent(out Rotation? rotation)
                    ? Vector3.Transform(movementVector, rotation.Value)
                    : movementVector;

                translation.Value += listener.Sensitivity * movementVector;
            }
        }

        private static Vector3 GetMovementVector(float deltaTime)
        {
            Vector3 movementVector = Vector3.Zero;

            if (InputManager.Instance.IsKeyPressed(Key.W)) movementVector += Vector3.UnitZ * deltaTime;

            if (InputManager.Instance.IsKeyPressed(Key.S)) movementVector -= Vector3.UnitZ * deltaTime;

            if (InputManager.Instance.IsKeyPressed(Key.A)) movementVector += Vector3.UnitX * deltaTime;

            if (InputManager.Instance.IsKeyPressed(Key.D)) movementVector -= Vector3.UnitX * deltaTime;

            return movementVector;
        }
    }
}
