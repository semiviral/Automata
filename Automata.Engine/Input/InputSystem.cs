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
            Vector2<float> relative_mouse_position = InputManager.Instance.GetMousePositionCenterRelative(0) * (float)delta.TotalSeconds;

            // invert axis for proper rotation
            // without this line, the x-axis rotation will be backwards
            relative_mouse_position = relative_mouse_position.WithX(-relative_mouse_position.X);

            if (Vector.All(relative_mouse_position == Vector2<float>.Zero))
            {
                return;
            }

            foreach ((Transform transform, MouseListener mouse_listener) in entityManager.GetComponents<Transform, MouseListener>())
            {
                mouse_listener.AccumulatedAngles += relative_mouse_position * mouse_listener.Sensitivity;
                Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, mouse_listener.AccumulatedAngles.X);
                Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, mouse_listener.AccumulatedAngles.Y);
                transform.Rotation = yaw * pitch;
            }

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionCenterRelative(0, Vector2<float>.Zero);
        }

        private static void HandleKeyboardListeners(EntityManager entityManager, TimeSpan delta)
        {
            Vector3 movement_vector = -GetMovementVector((float)delta.TotalSeconds);

            if (movement_vector == Vector3.Zero)
            {
                return;
            }

            foreach ((Transform transform, KeyboardListener listener) in entityManager.GetComponents<Transform, KeyboardListener>())
            {
                transform.Translation += (listener.Sensitivity * Vector3.Transform(movement_vector, transform.Rotation)).AsGeneric<float>();
            }
        }

        private static Vector3 GetMovementVector(float deltaTime)
        {
            Vector3 movement_vector = Vector3.Zero;

            if (InputManager.Instance.IsKeyPressed(Key.W))
            {
                movement_vector += Vector3.UnitZ * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.S))
            {
                movement_vector -= Vector3.UnitZ * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.A))
            {
                movement_vector += Vector3.UnitX * deltaTime;
            }

            if (InputManager.Instance.IsKeyPressed(Key.D))
            {
                movement_vector -= Vector3.UnitX * deltaTime;
            }

            return movement_vector;
        }
    }
}
