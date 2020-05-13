#region

using System.Diagnostics;
using Automata.Singletons;

#endregion

namespace Automata.Core.Systems
{
    public class ViewDoRenderSystem : ComponentSystem
    {
        public ViewDoRenderSystem()
        {
            GameWindow.Validate();
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            Debug.Assert(GameWindow.Instance != null);
            Debug.Assert(GameWindow.Instance.Window != null);

            GameWindow.Instance.Window.DoRender();
        }
    }
}
