#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Systems;

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

            Vector2 offset = InputManager.Instance.GetMousePositionRelative();

            // if offset is zero, the mouse has not moved, so return
            if (offset == Vector2.Zero)
            {
                return;
            }

            offset = InputManager.Instance.GetMousePositionRelative();
            // convert to axis angles (a la yaw/pitch/roll)
            Vector3 axisAngles = new Vector3(offset.Y, offset.X, 0f) * (float)delta.TotalSeconds;

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
