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
            if (!AutomataWindow.Instance.Focused) return;

            Vector2 relativeMousePosition = InputManager.Instance.GetMousePositionCenterRelative(0) * (float)delta.TotalSeconds;
            relativeMousePosition.X = -relativeMousePosition.X; // invert y axis for proper rotation

            // if offset is zero, the mouse has not moved, so return
            if (relativeMousePosition == Vector2.Zero) return;

            foreach ((Rotation rotation, MouseListener mouseListener) in entityManager.GetComponents<Rotation, MouseListener>())
            {
                rotation.AccumulateAngles(relativeMousePosition * mouseListener.Sensitivity);

                const string format = "{0} {1}";
                AutomataWindow.Instance.Title = string.Format(format, AutomataWindow.Instance.Title, Vector3.Transform(Vector3.UnitZ, rotation.Value));
            }

            // reset mouse position to center of screen
            InputManager.Instance.SetMousePositionRelative(0, Vector2.Zero);
        }
    }
}
