#region

using System;
using System.Diagnostics;
using Automata.Singletons;

#endregion

namespace Automata.Core.Systems
{
    public class ViewDoUpdateSystem : ComponentSystem
    {
        public ViewDoUpdateSystem()
        {
            GameWindow.Validate();
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindow.Instance.Window.DoEvents();
            GameWindow.Instance.Window.DoUpdate();
        }
    }
}
