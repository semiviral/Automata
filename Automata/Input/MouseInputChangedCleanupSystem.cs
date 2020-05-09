using System.Linq;
using Automata.Core;
using Serilog;

namespace Automata.Input
{
    public class MouseInputChangedCleanupSystem : ComponentSystem
    {
        public MouseInputChangedCleanupSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(MouseInput)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (MouseInput mouseInput in entityManager.GetComponents<MouseInput>().Where(mouseInput => mouseInput.Changed))
            {
                Log.Information(mouseInput.Value.ToString());
                mouseInput.Changed = false;
            }
        }
    }
}
