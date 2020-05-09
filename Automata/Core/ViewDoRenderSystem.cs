namespace Automata.Core
{
    /// <summary>
    ///     Runs the DoRender() method on all <see cref="WindowViewComponent" />.
    /// </summary>
    public class ViewDoRenderSystem : ComponentSystem
    {
        public ViewDoRenderSystem()
        {
            UtilizedComponentTypes = new[]
            {
                typeof(WindowViewComponent)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (WindowViewComponent windowViewComponent in entityManager.GetComponents<WindowViewComponent>())
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
