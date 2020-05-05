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

        public override void Update()
        {
            foreach (WindowViewComponent windowViewComponent in EntityManager.GetComponents<WindowViewComponent>())
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
