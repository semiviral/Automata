#region

using System;
using Automata.Core.Components;
using Automata.Core.Systems;

#endregion

namespace Automata.Core
{
    public class ComponentChangedCleanupSystem : ComponentSystem
    {
        public ComponentChangedCleanupSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation),
                typeof(Translation),
            };
        }

        public override void Update(EntityManager entityManager, TimeSpan delta)
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
