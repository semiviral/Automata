#region

using System.Numerics;
using Automata.Core;
using Silk.NET.Input.Common;

#endregion

namespace Automata.Input
{
    public class KeyboardInputTranslationSystem : ComponentSystem
    {
        public KeyboardInputTranslationSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(KeyboardInput),
                typeof(KeyboardInputTranslation)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            foreach ((KeyboardInput keyboardInput, KeyboardInputTranslation inputTranslation) in
                entityManager.GetComponents<KeyboardInput, KeyboardInputTranslation>())
            {
                Vector3 inputTranslationValue = Vector3.Zero;

                if (keyboardInput.KeysDown.Contains(Key.D))
                {
                    inputTranslationValue.X += 1f;
                }

                if (keyboardInput.KeysDown.Contains(Key.A))
                {
                    inputTranslationValue.X += -1f;
                }

                if (keyboardInput.KeysDown.Contains(Key.W))
                {
                    inputTranslationValue.Z += 1f;
                }

                if (keyboardInput.KeysDown.Contains(Key.S))
                {
                    inputTranslationValue.Z += -1f;
                }

                inputTranslation.Input = inputTranslationValue;
            }
        }
    }
}
