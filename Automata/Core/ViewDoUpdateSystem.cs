namespace Automata.Core
{
    /// <summary>
    ///     Runs the DoUpdate() method on all <see cref="WindowViewComponent" />.
    /// </summary>
    public class ViewDoUpdateSystem : ComponentSystem
    {
        public ViewDoUpdateSystem()
        {
            UtilizedComponentTypes = new[]
            {
                typeof(WindowViewComponent)
            };
        }

        public override void Update(EntityManager entityManager, double deltaTime)
        {
            foreach (WindowViewComponent windowViewComponent in entityManager.GetComponents<WindowViewComponent>())
            {
                if (windowViewComponent.View.IsClosing)
                {
                    continue;
                }

                windowViewComponent.View.DoEvents();
                windowViewComponent.View.DoUpdate();
            }
        }
    }
}
