namespace Automata.Core
{
    public class TranslationChangedCleanupSystem : ComponentSystem
    {
        public TranslationChangedCleanupSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Translation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach (Translation translation in entityManager.GetComponents<Translation>())
            {
                translation.Changed = false;
            }
        }
    }
}
