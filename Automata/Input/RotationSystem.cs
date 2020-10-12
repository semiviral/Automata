#region

using System;
using System.Numerics;
using Automata.Components;
using Automata.Entities;
using Automata.Rendering;
using Automata.Rendering.GLFW;
using Automata.Systems;

#endregion

namespace Automata.Input
{
    public class RotationSystem : ComponentSystem
    {
        private const float _SENSITIVITY = 10f;

        public RotationSystem() => HandledComponents = new ComponentTypes(typeof(Rotation));

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!AutomataWindow.Instance.Focused)
            {
                return;
            }

            Vector2 offset = InputManager.Instance.GetMousePositionRelative();

            // if offset is zero, the mouse has not moved, so return
            if (offset == Vector2.Zero)
            {
                return;
            }

            offset = InputManager.Instance.GetMousePositionRelative();
            // convert to axis angles (a la yaw/pitch/roll)
            Vector3 axisAngles = new Vector3(offset.Y, offset.X, 0f) * (float)delta.TotalSeconds * (_SENSITIVITY / 10f);

            foreach ((Camera camera, Rotation rotation) in entityManager.GetComponents<Camera, Rotation>())
            {
                // accumulate angles
                camera.AccumulatedAngles += axisAngles;

                // create quaternions based on local angles
                Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, camera.AccumulatedAngles.X);
                Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, camera.AccumulatedAngles.Y);

                // rotate around (pitch as global) and (yaw as local)
                rotation.Value = pitch * yaw;
            }

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionRelative(0, Vector2.Zero);
        }
    }
}
