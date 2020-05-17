#region

using System;
using System.Diagnostics;

#endregion

namespace Automata.GLFW
{
    public class ViewDoUpdateSystem : ComponentSystem
    {
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!GameWindow.TryValidate())
            {
                return;
            }

            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindow.Instance.Window.DoEvents();
            GameWindow.Instance.Window.DoUpdate();
        }
    }
}
