#region

using Automata.Core.Components;

#endregion

namespace Automata.Core.Systems
{
    public class RotationChangedCleanupSystem : ComponentSystem
    {
        public RotationChangedCleanupSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Rotation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Rotation rotation in entityManager.GetComponents<Rotation>())
            {
                if (!rotation.Changed)
                {
                    continue;
                }

                rotation.Changed = false;
            }
        }
    }
}
