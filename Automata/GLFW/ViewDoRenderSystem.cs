#region

using System;
using System.Diagnostics;

#endregion

namespace Automata.GLFW
{
    public class ViewDoRenderSystem : ComponentSystem
    {
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (!GameWindow.TryValidate())
            {
                return;
            }

            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindow.Instance.Window.DoRender();
        }
    }
}
