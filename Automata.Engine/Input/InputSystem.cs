using System;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Numerics;
using Silk.NET.Input;
using Vector = Automata.Engine.Numerics.Vector;

namespace Automata.Engine.Input
{
    public class InputSystem : ComponentSystem
    {
        public InputSystem(World world) : base(world) { }

        public override void Registered(EntityManager entityManager) =>
            AutomataWindow.Instance.FocusChanged += (_, focused) => Enabled = focused;

        [HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(MouseListener)),
         HandledComponents(EnumerationStrategy.All, typeof(Transform), typeof(KeyboardListener))]
        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            HandleMouseListeners(entityManager, delta);
            HandleKeyboardListeners(entityManager, delta);

            return ValueTask.CompletedTask;
        }

        private static void HandleMouseListeners(EntityManager entityManager, TimeSpan delta)
        {
            Vector2<float> relativeMousePosition = InputManager.Instance.GetMousePositionCenterRelative(0) * (float)delta.TotalSeconds;

            // invert axis for proper rotation
            // without this line, the x-axis rotation will be backwards
            relativeMousePosition = relativeMousePosition.WithX(-relativeMousePosition.X);

            if (Vector.All(relativeMousePosition == Vector2<float>.Zero))
            {
                return;
            }

            foreach ((Transform transform, MouseListener mouseListener) in entityManager.GetComponents<Transform, MouseListener>())
            {
                mouseListener.AccumulatedAngles += relativeMousePosition * mouseListener.Sensitivity;
                Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, mouseListener.AccumulatedAngles.X);
                Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, mouseListener.AccumulatedAngles.Y);
                transform.Rotation = yaw * pitch;
            }

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionCenterRelative(0, Vector2<float>.Zero);
        }

        private static void HandleKeyboardListeners(EntityManager entityManager, TimeSpan delta)
        {
            Vector3 movementVector = -GetMovementVector((float)delta.TotalSeconds);

            if (movementVector == Vector3.Zero)
            {
                return;
            }

            foreach ((Transform transform, KeyboardListener listener) in entityManager.GetComponents<Transform, KeyboardListener>())
            {
                transform.Translation += listener.Sensitivity * Vector3.Transform(movementVector, transform.Rotation);
            }
        }

        private static Vector3 GetMovementVector(float deltaTime)
        {
            Vector3 movementVector = Vector3.Zero;

            if (InputManager.Instance.IsKeyPressed(Key.W))
            {
                movementVector += Vector3.UnitZ * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.S))
            {
                movementVector -= Vector3.UnitZ * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.A))
            {
                movementVector += Vector3.UnitX * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.D))
            {
                movementVector -= Vector3.UnitX * deltaTime;
            }

            return movementVector;
        }
    }
}
