#region

using Automata.Core.Components;
using Automata.Rendering;

#endregion

namespace Automata.Core.Systems
{
    public class ComponentChangedCleanupSystem : ComponentSystem
    {
        public ComponentChangedCleanupSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation),
                typeof(Translation),
                typeof(Camera),
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Rotation rotation in entityManager.GetComponents<Rotation>())
            {
                if (rotation.Changed)
                {
                    rotation.Changed = false;
                }
            }

            foreach (Translation translation in entityManager.GetComponents<Translation>())
            {
                if (translation.Changed)
                {
                    translation.Changed = false;
                }
            }
        }
    }
}
