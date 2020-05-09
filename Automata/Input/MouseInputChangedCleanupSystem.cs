#region

using System.Linq;
using Automata.Core;

#endregion

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
                if (!mouseInput.Changed)
                {
                    continue;
                }

                mouseInput.Changed = false;
            }
        }
    }
}
