#region

using Automata.Core;

#endregion

namespace Automata.Input
{
    public class KeyboardInputTranslationToTranslationSystem : ComponentSystem
    {
        public KeyboardInputTranslationToTranslationSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(KeyboardInputTranslation),
                typeof(Translation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((KeyboardInputTranslation inputTranslation, Translation translation) in entityManager
                .GetComponents<KeyboardInputTranslation, Translation>())
            {
                translation.Position += inputTranslation.Value * deltaTime;
            }
        }
    }
}
