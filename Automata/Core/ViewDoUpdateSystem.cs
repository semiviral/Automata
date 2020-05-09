namespace Automata.Core
{
    /// <summary>
    ///     Runs the DoUpdate() method on all <see cref="WindowIViewProvider" />.
    /// </summary>
    public class ViewDoUpdateSystem : ComponentSystem
    {
        public ViewDoUpdateSystem()
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

                windowViewComponent.View.DoEvents();
                windowViewComponent.View.DoUpdate();
            }
        }
    }
}
