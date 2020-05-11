using Automata.Core.Components;

namespace Automata.Core.Systems
{
    /// <summary>
    ///     Runs the DoRender() method on all <see cref="WindowIViewProvider" />.
    /// </summary>
    public class ViewDoRenderSystem : ComponentSystem
    {
        public ViewDoRenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(WindowIViewProvider)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (WindowIViewProvider windowViewComponent in entityManager.GetComponents<WindowIViewProvider>())
            {
                if (windowViewComponent.View.IsClosing)
                {
                    continue;
                }

                windowViewComponent.View.DoRender();
            }
        }
    }
}
