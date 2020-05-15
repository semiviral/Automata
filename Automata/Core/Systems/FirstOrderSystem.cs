#region

using System;
using Automata.Singletons;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Core.Systems
{
    public class FirstOrderSystem : ComponentSystem
    {
        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            if (Input.Instance.IsKeyPressed(Key.Escape))
            {
                GameWindow.Instance.Window.Close();
            }
        }
    }
}
