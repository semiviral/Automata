#region

using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Systems;
using Silk.NET.Input.Common;

#endregion


namespace Automata.Engine.Input
{
    public class InputSystem : ComponentSystem
    {
        public override void Registered(EntityManager entityManager) =>
            AutomataWindow.Instance.FocusChanged += (_, focused) => Enabled = focused;

        [HandledComponents(DistinctionStrategy.All, typeof(Rotation), typeof(MouseListener)),
         HandledComponents(DistinctionStrategy.All, typeof(Translation), typeof(KeyboardListener))]
        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            HandleMouseListeners(entityManager, delta);
            HandleKeyboardListeners(entityManager, delta);

            return ValueTask.CompletedTask;
        }

        private static void HandleMouseListeners(EntityManager entityManager, TimeSpan delta)
        {
            Vector2 relativeMousePosition = InputManager.Instance.GetMousePositionCenterRelative(0) * (float)delta.TotalSeconds;

            // invert axis for proper rotation
            // without this line, the x-axis rotation will be backwards
            relativeMousePosition.X = -relativeMousePosition.X;

            if (relativeMousePosition == Vector2.Zero) return;

            foreach ((Rotation rotation, MouseListener mouseListener) in entityManager.GetComponents<Rotation, MouseListener>())
                rotation.AccumulateAngles(relativeMousePosition * mouseListener.Sensitivity);

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionCenterRelative(0, Vector2.Zero);
        }

        private static void HandleKeyboardListeners(EntityManager entityManager, TimeSpan delta)
        {
            Vector3 movementVector = -GetMovementVector((float)delta.TotalSeconds);

            if (movementVector == Vector3.Zero) return;

            foreach ((IEntity entity, Translation translation, KeyboardListener listener) in entityManager.GetEntitiesWithComponents<Translation, KeyboardListener>())
                translation.Value += listener.Sensitivity
                                     * (entity.TryFind(out Rotation? rotation)
                                         ? Vector3.Transform(movementVector, rotation.Value)
                                         : movementVector);
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
