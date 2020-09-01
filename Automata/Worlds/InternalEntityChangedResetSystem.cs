#region

using System;
using Automata.Entity;
using Automata.System;

#endregion

namespace Automata.Worlds
{
    public class InternalEntityChangedResetSystem : ComponentSystem
    {
        public InternalEntityChangedResetSystem() => HandledComponentTypes = new ComponentTypes(typeof(Translation), typeof(Rotation));

        public override void Update(EntityManager entityManager, TimeSpan delta)
        {
            foreach (Translation translation in entityManager.GetComponents<Translation>())
            {
                translation.Changed = false;
            }

            foreach (Rotation rotation in entityManager.GetComponents<Rotation>())
            {
                rotation.Changed = false;
            }
        }
    }
}
