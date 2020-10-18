#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Systems;
using Serilog;

#endregion

namespace Automata.Engine.Input
{
    public class MouseRotationSystem : ComponentSystem
    {
        [HandlesComponents(DistinctionStrategy.All, typeof(Rotation), typeof(MouseListener))]
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!AutomataWindow.Instance.Focused)
            {
                return;
            }

            Vector2 relativeMousePosition = InputManager.Instance.GetMousePositionCenterRelative(0);

            // if offset is zero, the mouse has not moved, so return
            if (relativeMousePosition == Vector2.Zero)
            {
                return;
            }

            // convert to axis angles (a la yaw/pitch/roll)
            Vector3 axisAngles = new Vector3(-relativeMousePosition.Y, relativeMousePosition.X, 0f) * (float)delta.TotalSeconds;

            foreach ((Rotation rotation, MouseListener mouseListener) in entityManager.GetComponents<Rotation, MouseListener>())
            {
                // accumulate angles
                rotation.AccumulateAngles(axisAngles * mouseListener.Sensitivity);
            }

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionRelative(0, Vector2.Zero);
        }
    }
}
