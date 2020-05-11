#region

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Automata.Core;
using Automata.Core.Systems;
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
                typeof(KeyboardInput)
            };
        }

        public override void Update(EntityManager entityManager, float deltaTime)
        {
            List<IEntity> entities = entityManager.GetEntitiesWithComponents<KeyboardInput>().ToList();
            foreach (IEntity entity in entities)
            {
                KeyboardInput keyboardInput = entity.GetComponent<KeyboardInput>();
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

                if (inputTranslationValue == Vector3.Zero)
                {
                    if (entity.TryGetComponent(out KeyboardInputTranslation _))
                    {
                        entityManager.RemoveComponent<KeyboardInputTranslation>(entity);
                    }
                }
                else
                {
                    entityManager.RegisterComponent(entity, new KeyboardInputTranslation
                    {
                        Value = inputTranslationValue
                    });
                }
            }
        }
    }
}
