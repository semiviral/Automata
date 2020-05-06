using System.Numerics;
using Automata.Core;
using Automata.Input;
using Automata.Rendering;
using Silk.NET.Input.Common;

namespace AutomataTest
{
    public class InputCameraViewMoverSystem : ComponentSystem
    {
        public InputCameraViewMoverSystem()
        {
            UtilizedComponentTypes = new[]
            {
                typeof(CameraEntityComponent),
                typeof(RenderedShaderComponent),
                typeof(KeyboardInputComponent)
            };
        }

        public override void Update(EntityManager entityManager, double deltaTime)
        {
            foreach (IEntity entity in entityManager
                .GetEntitiesWithComponents<CameraEntityComponent, RenderedShaderComponent, KeyboardInputComponent>())
            {
                RenderedShaderComponent renderedShaderComponent = entity.GetComponent<RenderedShaderComponent>();
                KeyboardInputComponent keyboardInputComponent = entity.GetComponent<KeyboardInputComponent>();

                Vector3 modificationVector = Vector3.Zero;

                if (keyboardInputComponent.KeysDown.Contains(Key.D))
                {
                    modificationVector.X = 1f;
                }

                if (keyboardInputComponent.KeysDown.Contains(Key.A))
                {
                    modificationVector.X = -1f;
                }

                
            }
        }
    }
}
